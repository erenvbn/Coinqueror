using Coinqueror.MarketData.Interfaces;
using MongoDB.Driver;


namespace Coinqueror.MarketData.Models
{

    public class HistoricalHourlyCollection : IHistoricalHourlyCollection
    {
        private readonly IMongoCollection<HistorySpotKlineModel> _collection;

        public HistoricalHourlyCollection(IMongoCollection<HistorySpotKlineModel> collection)
        {
            _collection = collection;
        }

        public async Task<List<HistorySpotKlineModel>> GetAllAsync()
        {
            return await _collection.Find(Builders<HistorySpotKlineModel>.Filter.Empty).ToListAsync();
        }

        public async Task InsertManyAsync(List<HistorySpotKlineModel> records)
        {
            await _collection.InsertManyAsync(records);
        }

        public async Task<long> CountDocumentsAsync()
        {
            return await _collection.CountDocumentsAsync(Builders<HistorySpotKlineModel>.Filter.Empty);
        }
    }

}
