using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using EyeMezzexz.Data;

namespace MezzexEye.Middleware
{
    public class AutoRejectLeaveMiddleware
    {
        private readonly RequestDelegate _next;

        public AutoRejectLeaveMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            var today = DateTime.Now.Date;

            var expiredLeaves = await dbContext.OfficeLeave
                .Where(l => l.Status == "Pending" && l.EndDate < today)
                .ToListAsync();

            foreach (var leave in expiredLeaves)
            {
                leave.Status = "Rejected";
                leave.StatusChangeOn = DateTime.Now;
                leave.StatusChangeBy = "Auto Reject By System";
            }

            if (expiredLeaves.Any())
            {
                await dbContext.SaveChangesAsync();
            }

            await _next(context);
        }
    }
}
