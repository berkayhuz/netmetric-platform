// <copyright file="LeadCapturedNotificationHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.Extensions.Logging;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Services;

namespace NetMetric.CRM.LeadManagement.Application.Commands.Leads;

public sealed class LeadCapturedNotificationHandler(
    ILeadRoutingService routingService,
    ILeadScoringEngineService scoringEngineService,
    ILogger<LeadCapturedNotificationHandler> logger) : INotificationHandler<LeadCapturedNotification>
{
    public async Task Handle(LeadCapturedNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing post-capture workflow for LeadId: {LeadId}", notification.LeadId);

        // 1. Auto-Routing
        await routingService.RouteLeadAsync(notification.LeadId, cancellationToken);

        // 2. Initial Scoring
        await scoringEngineService.EvaluateAndScoreAsync(notification.LeadId, cancellationToken);
    }
}
