using System;
using System.Threading.Tasks;

namespace SurveyManagement.Core.Interfaces
{
    public interface IAuditService
    {
        Task LogAuditEventAsync(string userId, string action, string entityType, string entityId, string details);
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string userId = null, string entityType = null, string entityId = null, DateTime? startDate = null, DateTime? endDate = null);
    }

    public class AuditLog
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string Action { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }
} 