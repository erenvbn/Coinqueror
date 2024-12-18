using Binance.Net.Clients;
using Binance.Net.Enums;
using Coinqueror.MarketData.Interfaces;
using Coinqueror.MarketData.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coinqueror.MarketData.Operations
{
    public class CommonDataOperations
    {
        private readonly BinanceRestClient _binanceClient;
        private readonly IHistoricalFiveMinutesCollection _historicalFiveMinutesCollection;
        private readonly IHistoricalChangeCollection _historicalChangeCollection;
        private readonly IHistoricalHourlyCollection _historicalHourlyCollection;
        private readonly ILogger<CommonDataOperations> _logger;
        private readonly string name = $"[{nameof(CommonDataOperations)}]";

        public CommonDataOperations(
            BinanceRestClient binanceClient,
            IHistoricalFiveMinutesCollection historicalFiveMinutesCollection,
            IHistoricalChangeCollection historicalChangeCollection,
            IHistoricalHourlyCollection historicalHourlyCollection,
            ILogger<CommonDataOperations> logger)
        {
            _binanceClient = binanceClient;
            _historicalFiveMinutesCollection = historicalFiveMinutesCollection;
            _historicalChangeCollection = historicalChangeCollection;
            _historicalHourlyCollection = historicalHourlyCollection;
            _logger = logger;
        }

        public async Task<List<HistorySpotKlineModel>> GetFiveMinuteHistoricalDataAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving data from the 5-minute historical collection.");
                return await _historicalFiveMinutesCollection.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving 5-minute historical data: {ex.Message}");
                throw;
            }
        }

        //public async Task<List<HistorySpotKlineModel>> GetHourlyHistoricalDataAsync()
        //{
        //    try
        //    {
        //        _logger.LogInformation("Retrieving data from the hourly historical collection.");
        //        return await _historicalHourlyCollection.GetAllAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error retrieving hourly historical data: {ex.Message}");
        //        throw;
        //    }
        //}

        //Run historical data job for a specific pair and interval
        public async Task RunHistoricalIntervalDataJob(
            string pairName,
            KlineInterval interval,
            IHistoricalCollection collection,
            int minutesBack, //Kline starting time in minutes
            int klineEndingTime,
            int limit)
        {
            // Fetch historical data from Binance
            var historicalDataToInsert = await GetHistoricalData(
                _binanceClient,
                pairName,
                interval,
                DateTime.UtcNow.AddMinutes(-minutesBack), //When the kline bars starting
                DateTime.UtcNow.AddMinutes(-klineEndingTime), //When the kline bars ending
                limit
            );

            _logger.LogDebug(
                name,
                $"PairName:{pairName}\n" +
                $"Interval:{interval.ToString()}\n" +
                $"MinutesBack:{minutesBack}\n" +
                $"Limit:{limit}\n" +
                $"HistoricalDataToInsert:{historicalDataToInsert.Count}"
            );

            _logger.LogDebug(name, $"{DateTime.UtcNow}, historical data for {pairName} with interval {interval} retrieved");

            if (historicalDataToInsert == null || historicalDataToInsert.Count == 0)
            {
                _logger.LogDebug(name, $"No historical data to insert.");
                return;
            }
            else
            {
                //await AddManyHistoricalDataToCollection(historicalDataToInsert, collection);
                await AddSingleHistoricalDataToCollection(historicalDataToInsert.First(), collection);
            }
        }

        // Generic method to get historical data for any interval
        public async Task<List<HistorySpotKlineModel>> GetHistoricalData(
            BinanceRestClient binanceClient,
            string symbol,
            KlineInterval interval,
            DateTime startTime,
            DateTime endTime,
            int limit = 50)
        {
            var result = await binanceClient.SpotApi.ExchangeData.GetKlinesAsync(symbol, interval, startTime, endTime);

            if (result.Success)
            {
                var historicalDataList = new List<HistorySpotKlineModel>();

                foreach (var data in result.Data)
                {
                    var klineData = new HistorySpotKlineModel
                    {
                        OpenTime = data.OpenTime,
                        OpenPriceUSDT = data.OpenPrice,
                        HighPriceUSDT = data.HighPrice,
                        LowPriceUSDT = data.LowPrice,
                        ClosePriceUSDT = data.ClosePrice,
                        Volume = data.Volume,
                        CloseTime = data.CloseTime,
                        QuoteVolume = data.QuoteVolume,
                        TradeCount = data.TradeCount,
                        TakerBuyBaseVolume = data.TakerBuyBaseVolume,
                        TakerBuyQuoteVolume = data.TakerBuyQuoteVolume,
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() // Save as milliseconds since epoch
                    };

                    historicalDataList.Add(klineData);
                }
                return historicalDataList;
            }
            else
            {
                _logger.LogError(name, $"Failed to retrieve data from Binance for symbol: {symbol}, interval: {interval}. Error: {result.Error?.Message}");
                return null;
            }
        }

        //Add Historical data to the collection method

        public async Task AddManyHistoricalDataToCollection(
            List<HistorySpotKlineModel> historicalDataList,
            IHistoricalCollection collection)
        {
            try
            {
                await collection.InsertManyAsync(historicalDataList);
                _logger.LogDebug(name, $"{historicalDataList.Count} number of historical data added to collection");
            }
            catch (Exception ex)
            {
                _logger.LogError(name, $"Error adding historical data to collection: {ex.Message}");
                throw;
            }
        }

        public async Task AddSingleHistoricalDataToCollection(
            HistorySpotKlineModel historicalDataSingle,
            IHistoricalCollection collection)
        {
            try
            {
                await collection.InsertOneAsync(historicalDataSingle);
                _logger.LogDebug(name, $"1 number of historical data added to collection");
            }
            catch (Exception ex)
            {
                _logger.LogError(name, $"Error adding historical data to collection: {ex.Message}");
                throw;
            }
        }
    }
}
