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

        public FiveMinutesScheduledJobService(
            ILogger<FiveMinutesScheduledJobService> logger,
            CommonDataOperations dataOperations,
            IHistoricalFiveMinutesCollection historicalFiveMinutesCollection,
            string pairNames = "BTCUSDT" // Default value provided in parameter
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
                    _logger.LogDebug("5-minute job started successfully");

                    await _dataOperations.RunHistoricalIntervalDataJob<HistorySpotKlineModel>(
                        _pairNames,
                        _interval,
                        _historicalFiveMinutesCollection,
                        _minutesBack,
                        _limit
                    );

                    _logger.LogDebug("5-minute job completed successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in 5-minute job: {ex.Message}");
                    _logger.LogError(ex, "An error occurred while processing the 5-minute job");
                }

                // Wait until the next 5-minute interval
                await HelperStaticWorkers.WaitUntilNextXMinuteInterval(stoppingToken, 5);
            }
        }
    }

}
