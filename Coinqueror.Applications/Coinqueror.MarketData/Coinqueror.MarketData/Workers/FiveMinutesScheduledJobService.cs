using Binance.Net.Enums;
using Coinqueror.MarketData.Interfaces;
using Coinqueror.MarketData.Models;
using Coinqueror.MarketData.Operations;
using Coinqueror.MarketData.Workers.HelperStaticWorker;

namespace Coinqueror.MarketData.Workers
{
    public class FiveMinutesScheduledJobService : BackgroundService
    {
        private readonly ILogger<FiveMinutesScheduledJobService> _logger;
        private readonly CommonDataOperations _dataOperations;
        private readonly IHistoricalFiveMinutesCollection _historicalFiveMinutesCollection;
        private readonly string _pairNames; // Removed default value from field
        private readonly KlineInterval _interval = KlineInterval.FiveMinutes;
        private readonly int _minutesBack = 5;
        private readonly int _limit = 50;
        private readonly string name = $"[{nameof(FiveMinutesScheduledJobService)}]";


        public FiveMinutesScheduledJobService(
            ILogger<FiveMinutesScheduledJobService> logger,
            CommonDataOperations dataOperations,
            IHistoricalFiveMinutesCollection historicalFiveMinutesCollection,
            string pairNames = "BTCUSDT"
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataOperations = dataOperations ?? throw new ArgumentNullException(nameof(dataOperations));
            _historicalFiveMinutesCollection = historicalFiveMinutesCollection;
            _pairNames = pairNames; // Initialize from parameter
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Run your job
                    _logger.LogDebug(name, $"5-minute job started successfully");

                    await _dataOperations.RunHistoricalIntervalDataJob(
                        _pairNames,
                        _interval,
                        _historicalFiveMinutesCollection,
                        _minutesBack,
                        _limit
                    );

                    _logger.LogDebug(name, "5-minute job completed successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(name, $"Error in 5-minute job: {ex.Message}");
                }

                // Wait until the next 5-minute interval
                await HelperStaticWorkers.WaitUntilNextXMinuteInterval(stoppingToken, 5);
            }
        }
    }

}
