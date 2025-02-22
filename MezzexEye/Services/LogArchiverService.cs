using EyeMezzexz.Data;
using Microsoft.EntityFrameworkCore;

namespace MezzexEye.Services
{
    public class LogArchiverService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public LogArchiverService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var archiveDate = DateTime.UtcNow.AddMonths(-6); // Archive logs older than 6 months
                var oldLogs = await context.UserLog.Where(l => l.Timestamp < archiveDate).ToListAsync();

                // Move to an archive or delete
                context.UserLog.RemoveRange(oldLogs);
                await context.SaveChangesAsync();

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken); // Run daily
            }
        }
    }

}
