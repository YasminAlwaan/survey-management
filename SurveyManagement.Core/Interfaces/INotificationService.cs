using System.Threading.Tasks;

namespace SurveyManagement.Core.Interfaces
{
    public interface INotificationService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendSmsAsync(string to, string message);
    }
} 