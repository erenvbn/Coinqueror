using MongoDB.Driver;
using Coinqueror.MarketData.Models;

namespace Coinqueror.MarketData.Interfaces
{
    public interface IHistoricalCollection
    {
        Task<List<HistorySpotKlineModel>> GetAllAsync();
        Task InsertManyAsync(IEnumerable<HistorySpotKlineModel> records);
        Task<long> CountDocumentsAsync();
        Task InsertOneAsync(HistorySpotKlineModel record);
    }
}
