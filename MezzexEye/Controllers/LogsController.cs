using EyeMezzexz.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MezzexEye.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class LogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string userId, string action, string logLevel, DateTime? fromDate, DateTime? toDate)
        {
            var logs = _context.UserLog.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                logs = logs.Where(l => l.UserId == userId);

            if (!string.IsNullOrEmpty(action))
                logs = logs.Where(l => l.Action == action);

            if (!string.IsNullOrEmpty(logLevel))
                logs = logs.Where(l => l.LogLevel == logLevel);

            if (fromDate.HasValue)
                logs = logs.Where(l => l.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                logs = logs.Where(l => l.Timestamp <= toDate.Value);

            return View(await logs.ToListAsync());
        }
    }

}
