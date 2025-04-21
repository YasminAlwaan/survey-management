using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SurveyManagement.Core.Interfaces;
using SurveyManagement.Infrastructure.Data;

namespace SurveyManagement.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditService> _logger;

        public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogAuditEventAsync(string userId, string action, string entityType, string entityId, string details)
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Audit event logged: {action} on {entityType} {entityId} by user {userId}");
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
            string userId = null,
            string entityType = null,
            string entityId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(l => l.UserId == userId);

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(l => l.EntityType == entityType);

            if (!string.IsNullOrEmpty(entityId))
                query = query.Where(l => l.EntityId == entityId);

            if (startDate.HasValue)
                query = query.Where(l => l.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(l => l.Timestamp <= endDate.Value);

            return await query.OrderByDescending(l => l.Timestamp).ToListAsync();
        }
    }
} 