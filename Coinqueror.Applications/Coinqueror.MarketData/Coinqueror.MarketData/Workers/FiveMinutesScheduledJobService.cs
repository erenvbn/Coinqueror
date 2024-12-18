using Binance.Net.Enums;
using Coinqueror.MarketData.Interfaces;
using Coinqueror.MarketData.Operations;
using Coinqueror.MarketData.Workers.HelperStaticWorker;

namespace Coinqueror.MarketData.Workers
{
    public class FiveMinutesScheduledJobService : BackgroundService
    {
        private readonly ILogger<FiveMinutesScheduledJobService> _logger;
        private readonly CommonDataOperations _dataOperations;
        private readonly IHistoricalFiveMinutesCollection _historicalFiveMinutesCollection;
        private readonly IHistoricalChangeCollection _historicalChangeCollection;
        private readonly List<string> _pairNames;
        private readonly List<KlineInterval> _intervals;
        private int _minutesBack;
        private int _klineEndingTime;
        private readonly int _limit = 50;
        private readonly int _backgroundWorkerRepetitionInterval = 5;
        private readonly string name = $"[{nameof(FiveMinutesScheduledJobService)}]";


        public FiveMinutesScheduledJobService(
            ILogger<FiveMinutesScheduledJobService> logger,
            CommonDataOperations dataOperations,
            IHistoricalFiveMinutesCollection historicalFiveMinutesCollection,
            IHistoricalChangeCollection historicalChangeCollection
            //List<string> pairNames
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataOperations = dataOperations ?? throw new ArgumentNullException(nameof(dataOperations));
            _historicalFiveMinutesCollection = historicalFiveMinutesCollection;
            _historicalChangeCollection = historicalChangeCollection;
            _intervals = new List<KlineInterval>() { KlineInterval.OneWeek, KlineInterval.OneMonth };
            _pairNames =  new List<string>() { "BTCUSDT", "ETHUSDT" };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //Buraya 5 dakikada bir yapılan işlemler eklenecek
            //pairnames liste halinde alınarak işlemler arka arkaya çalıştırabilir
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Run your job
                    _logger.LogDebug(name, $"5-minute job started successfully");

                    _logger.LogDebug(name, "Running 5-minute job");
                    foreach (string pairName in _pairNames)
                    {
                        _logger.LogDebug(name, $"Getting 5-minute before market data for pairName: {pairName}");
                        await _dataOperations.RunHistoricalIntervalDataJob(
                            pairName,
                            KlineInterval.FiveMinutes, //Get five minutes data
                            _historicalFiveMinutesCollection, //save it into 5-minute collection in mongodb
                            minutesBack: 5, //Run in every five minutes
                            klineEndingTime: 0,
                            _limit
                        );
                    }

                    foreach (string pairName in _pairNames)
                    {
                        foreach (var interval in _intervals)
                        {
                            //Adding a switch here for different intervals to change _minutesBack and _klineEndingTime
                            var intervalTuple = SwitchForKlineInterval(interval);
                            if (intervalTuple.Item1 != 0 && intervalTuple.Item2 != 0)
                            {
                                _logger.LogDebug(name, $"Getting interval: {interval} before market data for pairName: {pairName}");
                                await _dataOperations.RunHistoricalIntervalDataJob(
                                    pairName,
                                    interval, //Get five minutes, 1 week and 1 month data
                                    _historicalChangeCollection, //save it into 5-minute collection in mongodb
                                    intervalTuple.Item1, //Start date for searching binance data as minutesback
                                    intervalTuple.Item2, //Ending date for searching binance data as klineEndingTime
                                    _limit
                                );
                            }
                        }
                    }

                    _logger.LogDebug(name, "5-minute job completed successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(name, $"Error in 5-minute job: {ex.Message}");
                }

                // Wait until the next 5-minute interval, run at every five minutes
                await HelperStaticWorkers.WaitUntilNextXMinuteInterval(stoppingToken, _backgroundWorkerRepetitionInterval);
            }
        }

        public Tuple<int, int> SwitchForKlineInterval(KlineInterval klineInterval)
        {
            int _minutesBack = 0;
            int _klineEndingTime = 0;

            switch (klineInterval)
            {
                case KlineInterval.OneWeek:
                    _minutesBack = 7 * 24 * 60; // 7 days * 24 hours * 60 minutes
                    _klineEndingTime = _minutesBack -5; // Add 1 minute to the back time
                    break;

                case KlineInterval.OneMonth:
                    _minutesBack = 30 * 24 * 60; // 30 days * 24 hours * 60 minutes
                    _klineEndingTime = _minutesBack -5;
                    break;

                case KlineInterval.ThreeDay:
                    _minutesBack = 3 * 24 * 60;
                    _klineEndingTime = _minutesBack -5;
                    break;
            }

            return Tuple.Create(_minutesBack, _klineEndingTime);
        }
    }

}
