namespace Coinqueror.MarketData.Models
{
    public class MongoDbSettings
    {
        public required string ConnectionUri { get; set; }
        public required string DatabaseName { get; set; }
        public required string HistoricalFiveMinutesCollection { get; set; }
        public required string HistoricalHourlyCollection { get; set; }
    }

}
