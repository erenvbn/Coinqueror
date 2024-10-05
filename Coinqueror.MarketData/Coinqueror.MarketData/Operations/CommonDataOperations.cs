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
        private readonly IHistoricalHourlyCollection _historicalHourlyCollection;
        private readonly ILogger<CommonDataOperations> _logger;

        public CommonDataOperations(
            BinanceRestClient binanceClient,
            IHistoricalFiveMinutesCollection historicalFiveMinutesCollection,
            IHistoricalHourlyCollection historicalHourlyCollection,
            ILogger<CommonDataOperations> logger)
        {
            _binanceClient = binanceClient;
            _historicalFiveMinutesCollection = historicalFiveMinutesCollection;
            _historicalHourlyCollection = historicalHourlyCollection;
            _logger = logger;
        }

        public async Task ProcessHistoricalFiveMinutesDataAsync(List<HistorySpotKlineModel> historySpotKlineList)
        {
            try
            {
                //var historySpotKlineList = historicalData.Data.Select(historicalData => new HistorySpotKlineModel
                //{
                //    OpenTime = historicalData.OpenTime,
                //    OpenPriceUSDT = historicalData.OpenPrice,
                //    HighPriceUSDT = historicalData.HighPrice,
                //    LowPriceUSDT = historicalData.LowPrice,
                //    ClosePriceUSDT = historicalData.ClosePrice,
                //    Volume = historicalData.Volume,
                //    CloseTime = historicalData.CloseTime,
                //    QuoteVolume = historicalData.QuoteVolume,
                //    TradeCount = historicalData.TradeCount,
                //    TakerBuyBaseVolume = historicalData.TakerBuyBaseVolume,
                //    TakerBuyQuoteVolume = historicalData.TakerBuyQuoteVolume,
                //    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() // Store timestamp in milliseconds
                //}).ToList();

                await _historicalFiveMinutesCollection.InsertManyAsync(historySpotKlineList);
                _logger.LogInformation("Inserted historical data into the 5-minute collection.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inserting 5-minute historical data: {ex.Message}");
                throw;
            }
        }

        //public async Task ProcessHistoricalHourlyDataAsync(HistoricalDataResponse historicalData)
        //{
        //    try
        //    {
        //        var historySpotKlineList = historicalData.Data.Select(historicalData => new HistorySpotKlineModel
        //        {
        //            OpenTime = historicalData.OpenTime,
        //            OpenPriceUSDT = historicalData.OpenPrice,
        //            HighPriceUSDT = historicalData.HighPrice,
        //            LowPriceUSDT = historicalData.LowPrice,
        //            ClosePriceUSDT = historicalData.ClosePrice,
        //            Volume = historicalData.Volume,
        //            CloseTime = historicalData.CloseTime,
        //            QuoteVolume = historicalData.QuoteVolume,
        //            TradeCount = historicalData.TradeCount,
        //            TakerBuyBaseVolume = historicalData.TakerBuyBaseVolume,
        //            TakerBuyQuoteVolume = historicalData.TakerBuyQuoteVolume,
        //            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() // Store timestamp in milliseconds
        //        }).ToList();

        //        await _historicalHourlyCollection.InsertManyAsync(historySpotKlineList);
        //        _logger.LogInformation("Inserted historical data into the hourly collection.");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error inserting hourly historical data: {ex.Message}");
        //        throw;
        //    }
        //}

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

        public async Task RunHistoricalIntervalDataJob(
            string pairName,
            KlineInterval interval,
            IHistoricalCollection collection,
            int minutesBack,
            int limit)
        {
            // Fetch historical data from Binance
            var historicalDataToInsert = await GetHistoricalData(
                _binanceClient,
                pairName,
                interval,
                DateTime.UtcNow.AddMinutes(-minutesBack),
                DateTime.UtcNow,
                limit
            );

            Console.WriteLine($"{DateTime.UtcNow}, historical data for {pairName} with interval {interval} retrieved");

            if (historicalDataToInsert == null || !historicalDataToInsert.Any())
            {
                Console.WriteLine("No historical data to insert.");
                return;
            }

            // Insert data directly as HistorySpotKlineModel
            await collection.InsertManyAsync(historicalDataToInsert);
            Console.WriteLine($"Historical data job for {pairName} with interval {interval} finished");
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
                List<HistorySpotKlineModel> historicalData = new List<HistorySpotKlineModel>();

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

                    historicalData.Add(klineData);
                }
                return historicalData;
            }
            else
            {
                _logger.LogError($"Failed to retrieve data from Binance for symbol: {symbol}, interval: {interval}. Error: {result.Error?.Message}");
                return null;
            }
        }
    }
}
