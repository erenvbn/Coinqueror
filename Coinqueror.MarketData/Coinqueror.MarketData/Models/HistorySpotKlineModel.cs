using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Coinqueror.MarketData.Models
{
    public class HistorySpotKlineModel
    {
        [BsonElement("OpenTime")]
        public DateTime OpenTime { get; set; }

        [BsonElement("OpenPriceUSDT")]
        public decimal OpenPriceUSDT { get; set; }

        [BsonElement("HighPriceUSDT")]
        public decimal HighPriceUSDT { get; set; }

        [BsonElement("LowPriceUSDT")]
        public decimal LowPriceUSDT { get; set; }

        [BsonElement("ClosePriceUSDT")]
        public decimal ClosePriceUSDT { get; set; }

        [BsonElement("Volume")]
        public decimal Volume { get; set; }

        [BsonElement("CloseTime")]
        public DateTime CloseTime { get; set; }

        [BsonElement("QuoteVolume")]
        public decimal QuoteVolume { get; set; }

        [BsonElement("TradeCount")]
        public int TradeCount { get; set; }

        [BsonElement("TakerBuyBaseVolume")]
        public decimal TakerBuyBaseVolume { get; set; }

        [BsonElement("TakerBuyQuoteVolume")]
        public decimal TakerBuyQuoteVolume { get; set; }

        [BsonElement("Timestamp")]
        public long Timestamp { get; set; }
    }
}
