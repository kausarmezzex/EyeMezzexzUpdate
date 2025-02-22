using EyeMezzexz.Models;

namespace MezzexEye.Services
{
    public interface ILogService
    {
        Task LogAsync(UserLog log, HttpContext context);
    }
}
