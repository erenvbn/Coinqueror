namespace Coinqueror.MarketData.Workers.HelperStaticWorker
{
    public static class HelperStaticWorkers
    {
        public static async Task WaitUntilNextXMinuteInterval(CancellationToken stoppingToken, int minuteInterval, ILogger logger = null)
        {
            var now = DateTime.UtcNow;
            var nextRun = now.AddMinutes(minuteInterval - (now.Minute % minuteInterval)).AddSeconds(-now.Second);
            var delay = nextRun - now;

            if (delay.TotalMilliseconds > 0)
            {
                logger?.LogDebug("Waiting for {delay} until next {minuteInterval}-minute interval.", delay, minuteInterval);
                await Task.Delay(delay, stoppingToken);
            }
        }
    }

}
