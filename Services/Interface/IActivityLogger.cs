// Services/Interface/IActivityLogger.cs
using System.Threading.Tasks;

namespace ProjectManagementSystem.Services.Interface
{
    public interface IActivityLogger
    {
        Task LogActivityAsync(string adminId, string action, string details, Microsoft.AspNetCore.Http.HttpContext httpContext);
    }
}