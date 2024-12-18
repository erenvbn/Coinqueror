using Coinqueror.UserService.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        //What this do is that 
        //1. It adds the connection string to the configuration
        //2. It adds the database context to the services

        //AppDbContext'i dependency injection container'a ekliyoruz (AddDbContext aracýlýðýyla)
        //AddDbContext'in postgresconnection stringini almasýný ve o database'i kullanmasýný söylüyoruz
        //Registering APPDbContext to dependency injection container using ADDDbContext, scoped service olarak ekliyor
        //Böylelikle her HTTP isteði için yeni bir AppDbContext instance oluþturulur ve yok edilir (using ile kullanýlýrsa)

        //builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection"));
            //options.EnableSensitiveDataLogging(); // Optional: Useful for debugging
            options.UseLoggerFactory(LoggerFactory.Create(logging => logging.AddConsole())); // Log to console
        });

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // Optional: Set to true for production with HTTPS
                options.SaveToken = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"], // Get from appsettings.json
                    ValidAudience = builder.Configuration["JwtSettings:Audience"], // Get from appsettings.json
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"])) // Get from appsettings.json
                };
            });

        //Adding user table database context

        builder.Services.AddControllers();
        // Learn more about configuring SwaggerS/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid JWT token",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
            });
        });


        builder.Services.AddSingleton(serviceProvider =>
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            return loggerFactory.CreateLogger("GlobalLogger"); // You can name your logger here
        });

        builder.Services.AddHostedService<TokenExpiryCheckerService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                //c.OAuthClientId("swagger-client-id"); // If you want to integrate OAuth2.0 or something similar
                c.OAuthAppName("Swagger UI");
            });
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();  // This will authenticate requests based on the JWT token
        app.UseAuthorization();   // Authorization comes after authentication

        app.MapControllers();

        //var secretKey = builder.Configuration["JwtSettings:SecretKey"];
        //Console.WriteLine($"Secret Key: {secretKey}");

        Console.WriteLine($"Coinqueror.UserService is starting");

        app.Run();
    }
}