// <copyright file="RegisterWebhookCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Persistence;
using NetMetric.CRM.IntegrationHub.Application.Security;
using NetMetric.CRM.IntegrationHub.Domain.Entities;
using NetMetric.Tenancy;

namespace NetMetric.CRM.IntegrationHub.Application.Commands.RegisterWebhook;

public sealed class RegisterWebhookCommandHandler(
    IIntegrationHubDbContext dbContext,
    ITenantContext tenantContext) : IRequestHandler<RegisterWebhookCommand, Guid>
{
    public async Task<Guid> Handle(RegisterWebhookCommand request, CancellationToken cancellationToken)
    {
        var tenantId = TenantRequestGuard.Resolve(tenantContext, request.TenantId);
        var entity = new WebhookSubscription(
            tenantId,
            request.Name,
            request.EventKey,
            request.TargetUrl,
            request.SecretKey,
            request.TimeoutSeconds,
            request.MaxAttempts);
        await dbContext.WebhookSubscriptions.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
