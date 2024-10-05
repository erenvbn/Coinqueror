using Binance.Net.Clients;
using Coinqueror.MarketData.Interfaces;
using Coinqueror.MarketData.Models;
using Coinqueror.MarketData.Operations;
using Coinqueror.MarketData.Workers;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Build the configuration
var configuration = builder.Configuration;

// Bind MongoDB and Binance settings from appsettings.json
var mongoDbSettings = new MongoDbSettings
{
    ConnectionUri = configuration["Mongo:ConnectionString"],
    DatabaseName = configuration["Mongo:DatabaseName"],
    HistoricalFiveMinutesCollection = configuration["Mongo:HistoricalFiveMinutesCollection"],
    HistoricalHourlyCollection = configuration["Mongo:HistoricalOneHourCollection"]
};

// Add MongoDB and Binance services
builder.Services.AddSingleton(mongoDbSettings);

builder.Services.AddSingleton<BinanceRestClient>(provider =>
{
    return new BinanceRestClient(options =>
    {
        options.ApiCredentials = new CryptoExchange.Net.Authentication.ApiCredentials(
            key: configuration["Binance:ApiKey"],
            secret: configuration["Binance:ApiSecret"]
        );
    });
});

builder.Services.AddSingleton<MongoClient>(provider =>
{
    Console.WriteLine($"ConnectionUri: {mongoDbSettings.ConnectionUri}");
    return GetMongoDBClient(mongoDbSettings.ConnectionUri);
});

builder.Services.AddSingleton<IMongoDatabase>(provider =>
{
    var mongoClient = provider.GetRequiredService<MongoClient>();
    Console.WriteLine($"DatabaseName: {mongoDbSettings.DatabaseName}");
    return GetMongoDatabase(mongoClient, mongoDbSettings.DatabaseName).GetAwaiter().GetResult();
});

// Register collections for MongoDB
builder.Services.AddSingleton<IHistoricalFiveMinutesCollection>(provider =>
{
    var mongoDatabase = provider.GetRequiredService<IMongoDatabase>();
    var collection = mongoDatabase.GetCollection<HistorySpotKlineModel>(mongoDbSettings.HistoricalFiveMinutesCollection);
    return new HistoricalFiveMinutesCollection(collection);
});

builder.Services.AddSingleton<IHistoricalHourlyCollection>(provider =>
{
    var mongoDatabase = provider.GetRequiredService<IMongoDatabase>();
    var collection = mongoDatabase.GetCollection<HistorySpotKlineModel>(mongoDbSettings.HistoricalHourlyCollection);
    return new HistoricalHourlyCollection(collection);
});

// Add operations class
builder.Services.AddSingleton<CommonDataOperations>(provider =>
{
    var binanceClient = provider.GetRequiredService<BinanceRestClient>();
    var historicalFiveMinutesCollection = provider.GetRequiredService<IHistoricalFiveMinutesCollection>();
    var historicalHourlyCollection = provider.GetRequiredService<IHistoricalHourlyCollection>();

    return new CommonDataOperations(
        binanceClient,
        historicalFiveMinutesCollection,
        historicalHourlyCollection,
        provider.GetRequiredService<ILogger<CommonDataOperations>>()
    );
});

// Add scheduled job services
builder.Services.AddHostedService<FiveMinutesScheduledJobService>();

// Logging
builder.Services.AddLogging(configure => configure.AddConsole());

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

// MongoDB helper methods
static MongoClient GetMongoDBClient(string connectionUri)
{
    var settings = MongoClientSettings.FromConnectionString(connectionUri);
    settings.ServerApi = new ServerApi(ServerApiVersion.V1);
    return new MongoClient(settings);
}

static async Task<IMongoDatabase> GetMongoDatabase(MongoClient mongoClient, string databaseName)
{
    var database = mongoClient.GetDatabase(databaseName);
    return database;
}
