// Services/ActivityLogger.cs
using Microsoft.AspNetCore.Http;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Services.Interface;
using System;
using System.Threading.Tasks;

namespace ProjectManagementSystem.Services
{
    public class ActivityLogger : IActivityLogger
    {
        private readonly ApplicationDbContext _context;

        public ActivityLogger(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogActivityAsync(
            string adminId,
            string action,
            string nameUsed,
            HttpContext httpContext)
        {
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

            var log = new AdminActivityLog
            {
                AdminId = adminId,
                LoggedName = nameUsed,  // Direct storage of the name
                Action = action,
                Details = $"{nameUsed} performed {action}",
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            };

            _context.AdminActivityLog.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}