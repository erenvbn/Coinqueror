using Coinqueror.MarketData.Interfaces;
using MongoDB.Driver;


namespace Coinqueror.MarketData.Models
{

    public class HistoricalChangeCollection : IHistoricalChangeCollection
    {
        private readonly IMongoCollection<HistorySpotKlineModel> _collection;

        public HistoricalChangeCollection(IMongoCollection<HistorySpotKlineModel> collection)
        {
            _collection = collection;
        }

        public async Task<List<HistorySpotKlineModel>> GetAllAsync()
        {
            return await _collection.Find(Builders<HistorySpotKlineModel>.Filter.Empty).ToListAsync();
        }

        public async Task InsertManyAsync(IEnumerable<HistorySpotKlineModel> records)
        {
            await _collection.InsertManyAsync(records);
        }

        public async Task InsertOneAsync(HistorySpotKlineModel record)
        {
            await _collection.InsertOneAsync(record);
        }


        public async Task<long> CountDocumentsAsync()
        {
            return await _collection.CountDocumentsAsync(Builders<HistorySpotKlineModel>.Filter.Empty);
        }
    }

}
