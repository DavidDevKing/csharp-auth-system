using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace csharp_auth_system.CRON;

public class UnactivatedUserCleanup(ILogger<UnactivatedUserCleanup> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(6));

        do
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                int deletedCount = await context.Users
                    .Where(u => !u.IsActive && u.TokenExpires < DateTime.UtcNow)
                    .ExecuteDeleteAsync(token);
                logger.LogInformation($"Deleted {deletedCount} unactivated users");

            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to clean up users");
            }
        }
        while (await timer.WaitForNextTickAsync(token));
    }
}