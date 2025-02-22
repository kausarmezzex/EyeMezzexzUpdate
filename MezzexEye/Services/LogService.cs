using System.IdentityModel.Claims;
using EyeMezzexz.Data;
using EyeMezzexz.Models;
using Microsoft.EntityFrameworkCore;

namespace MezzexEye.Services
{
    public class LogService : ILogService
    {
        private readonly ApplicationDbContext _context;

        public LogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(UserLog log, HttpContext context)
        {
            // Get authenticated user
            var user = context.User;
            var userId = user?.Identity?.IsAuthenticated == true
                ? user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown User"
                : "System";
            _context.CurrentUserId = userId;

            // Save log
            await _context.UserLog.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }

}
