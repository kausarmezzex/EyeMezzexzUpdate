using EyeMezzexz.Data;
using EyeMezzexz.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class HolidayResetService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HolidayResetService> _logger;

    public HolidayResetService(IServiceProvider serviceProvider, ILogger<HolidayResetService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var currentDate = DateTime.UtcNow;

            // Check if the current month is April
            if (currentDate.Month == 4)
            {
                Console.WriteLine("Executing holiday reset for April...");
                _logger.LogInformation("Starting holiday reset process.");
                await ResetHolidaysAsync();
                _logger.LogInformation("Holiday reset process completed.");
            }
            else
            {
                Console.WriteLine($"Skipping reset, current month is {currentDate:MMMM}.");
            }

            // Wait until the start of the next month
            var firstOfNextMonth = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(1);
            var delayDuration = firstOfNextMonth - currentDate;

            await Task.Delay(delayDuration, stoppingToken);
        }
    }


    private async Task ResetHolidaysAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Fetch all user accounts with holiday details
        var userAccounts = await dbContext.UserAccountDetail
            .Include(u => u.ApplicationUser) // Include related ApplicationUser for country
            .ToListAsync();

        foreach (var userAccount in userAccounts)
        {
            var country = userAccount.ApplicationUser?.CountryName;

            if (country == "United Kingdom")
            {
                // Carry over remaining holiday hours
                if (userAccount.RemainingYearlyHours.HasValue)
                {
                    userAccount.CarryOverLeaveHours = userAccount.RemainingYearlyHours.Value;
                }

                // Reset remaining holiday hours to yearly allocated hours
                userAccount.RemainingYearlyHours = userAccount.YearlyLeaveHours;
                _logger.LogInformation($"Reset holiday hours for UserId: {userAccount.UserId} (UK). " +
                                        $"CarryOverHours: {userAccount.CarryOverLeaveHours}.");
            }
            else if (country == "India")
            {
                // Carry over remaining holiday days
                if (userAccount.RemainingYearlyLeave.HasValue)
                {
                    userAccount.CarryOverLeave = userAccount.RemainingYearlyLeave.Value;
                }

                // Reset remaining holiday days to yearly allocated days
                userAccount.RemainingYearlyLeave = userAccount.YearlyLeave;
                _logger.LogInformation($"Reset holiday days for UserId: {userAccount.UserId} (India). " +
                                        $"CarryOverDays: {userAccount.CarryOverLeave}.");
            }
            else
            {
                _logger.LogWarning($"Country not handled for UserId: {userAccount.UserId}. Skipping reset.");
            }
        }

        // Save changes to the database
        await dbContext.SaveChangesAsync();
    }
}
