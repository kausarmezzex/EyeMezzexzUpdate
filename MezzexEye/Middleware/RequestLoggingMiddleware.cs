using EyeMezzexz.Data;
using EyeMezzexz.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace MezzexEye.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        public RequestLoggingMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task Invoke(HttpContext context, ApplicationDbContext dbContext)
        {
            var user = context.User;

            // Skip logging for unauthenticated users
            if (user?.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown User";
            dbContext.CurrentUserId = userId;   
            var url = context.Request.Path.Value.ToLower(); // Normalize URL
            var method = context.Request.Method;

            // Check for recent GET request for the same URL
            if (method == HttpMethods.Get && _cache.TryGetValue($"GET:{userId}:{url}", out _))
            {
                await _next(context);
                return;
            }

            // Log request
            var log = new UserLog
            {
                UserId = userId,
                Action = method == HttpMethods.Post ? "Submit Form" : "Page Visit",
                URL = url,
                LogLevel = "Information",
                Timestamp = DateTime.UtcNow,
                Details = method == HttpMethods.Post
                    ? $"Form submitted at {url}"
                    : $"Visited {url}"
            };

            dbContext.UserLog.Add(log);
            await dbContext.SaveChangesAsync();

            // Cache GET request to avoid duplicate logs
            if (method == HttpMethods.Get)
            {
                _cache.Set($"GET:{userId}:{url}", true, TimeSpan.FromSeconds(10)); // Cache for 10 seconds
            }

            await _next(context);
        }
    }
}
