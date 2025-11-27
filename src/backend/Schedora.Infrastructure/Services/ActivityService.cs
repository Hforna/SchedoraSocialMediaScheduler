using Microsoft.Extensions.Logging;
using Schedora.Domain.Entities;
using Schedora.Domain.Interfaces;
using Schedora.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Schedora.Infrastructure.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly ILogger<IActivityLogService> _logger;
        private readonly IUnitOfWork _uow;
        private readonly IRequestService _requestService;

        public ActivityLogService(ILogger<IActivityLogService> logger, IUnitOfWork uow, IRequestService requestService)
        {
            _logger = logger;
            _uow = uow;
            _requestService = requestService;
        }

        public async Task LogAsync(long userId, string action, string entityType, long entityId, object? details = null, bool commit = true)
        {
            var requestIp = _requestService.GetRequestIpAddress();
            var userAgent = _requestService.GetUserAgent();

            string? detailsJson = null;
            if (details != null)
            {
                detailsJson = JsonSerializer.Serialize(details, new JsonSerializerOptions
                {
                    WriteIndented = false
                });
            }

            var log = ActivityLog.Create(userId, action, entityType, entityId, detailsJson, requestIp, userAgent);

            await _uow.GenericRepository.Add<ActivityLog>(log);
            if (commit)
                await _uow.Commit();
        }
    }
}
