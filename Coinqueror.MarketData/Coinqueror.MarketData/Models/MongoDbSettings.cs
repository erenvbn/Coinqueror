namespace Coinqueror.MarketData.Models
{
    public class MongoDbSettings
    {
        public string ConnectionUri { get; set; }
        public string DatabaseName { get; set; }
        public string HistoricalFiveMinutesCollection { get; set; }
        public string HistoricalHourlyCollection { get; set; }
    }

}
