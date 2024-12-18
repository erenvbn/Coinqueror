using Coinqueror.UserService.Data;
using Microsoft.EntityFrameworkCore;

public class TokenExpiryCheckerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TokenExpiryCheckerService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromDays(1); // Adjust as needed
    private readonly string name = "TokenExpiryCheckerService";

    public TokenExpiryCheckerService(IServiceProvider serviceProvider, ILogger<TokenExpiryCheckerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    //Burada .NET framework tarafından sağlanan BackgroundService sınıfını kalıtım alıyoruz
    //Bu sınıf, arka planda çalışan bir servis oluşturmak için kullanılır
    //ExecuteAsync metodu, servisin ana iş mantığını içerir
    //.NET Framework CancellationTokenSource sınıfı uygulama başlatıldığında oluşturulur ve uygulama kapatıldığında iptal edilir
    //CancellationTrigger nasıl çalışır, uygulama başlatıldığında bir CancellationTokenSource
    //nesnesi oluşturulur ve bu nesne ExecuteAsync metodu çalıştırılırken geçirilir

    //Eğer uygulama gracefuly bir şekilde kapatılırsa birçok event çalışır, bu eventler
    //.NET Framework CancellationTokenSource nesnesi üzerindeki Cancel metodunu çağırır
    //Bu metod, CancellationTokenSource nesnesi üzerindeki Token nesnesi üzerindeki IsCancellationRequested özelliğini true yapar
    //Bu özellik, ExecuteAsync metodu içerisindeki while döngüsünü kırar ve servisi durdurur

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Get the current time
            var currentTime = DateTime.UtcNow;

            // Calculate the time until the next run (2 AM)
            var nextRunTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 2, 0, 0, DateTimeKind.Utc);

            if (currentTime > nextRunTime) // If it's already past 2 AM, schedule for tomorrow
            {
                nextRunTime = nextRunTime.AddDays(1);
            }

            var timeUntilNextRun = nextRunTime - currentTime;

            // Wait until the next scheduled run
            await Task.Delay(timeUntilNextRun, stoppingToken);

            try
            {
                using (var appDbContextScope = _serviceProvider.CreateScope())
                {
                    var appDbContext = appDbContextScope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var expiredUsers = await appDbContext.Users
                        .Where(u => u.LastLoginExpiryDate < DateTime.UtcNow)
                        .ToListAsync(stoppingToken);

                    foreach (var user in expiredUsers)
                    {
                        user.Token = null; // or any other action to log out the user
                        user.LastLoginExpiryDate = null;
                        _logger.LogInformation(name, $"Logged out automatically user UserId:{user.Id}, UserName: {user.Username}, LastLoginExpiryDate: {user.LastLoginExpiryDate} due to expired token.");
                    }

                    await appDbContext.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking for expired tokens.");
            }
        }
    }
}
