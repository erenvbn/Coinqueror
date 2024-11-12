using Coinqueror.TradeShifter.Data;
using Coinqueror.TradeShifter.Models;
using Microsoft.EntityFrameworkCore;

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


//Adding user table database context

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ILogger>(serviceProvider =>
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
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
