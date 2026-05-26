// <copyright file="ValidateWebhookDeliveryCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Persistence;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Processing;
using NetMetric.CRM.IntegrationHub.Application.DTOs;
using NetMetric.CRM.IntegrationHub.Application.Security;
using NetMetric.CRM.IntegrationHub.Domain.Entities;
using NetMetric.Exceptions;
using NetMetric.Guards;
using NetMetric.Tenancy;

namespace NetMetric.CRM.IntegrationHub.Application.Commands.ValidateWebhookDelivery;

public sealed class ValidateWebhookDeliveryCommandHandler(
    IIntegrationHubDbContext dbContext,
    ITenantContext tenantContext,
    IIntegrationWebhookSecurityService webhookSecurityService) : IRequestHandler<ValidateWebhookDeliveryCommand, WebhookValidationResultDto>
{
    public async Task<WebhookValidationResultDto> Handle(ValidateWebhookDeliveryCommand request, CancellationToken cancellationToken)
    {
        var tenantId = TenantRequestGuard.Resolve(tenantContext, request.TenantId);
        var providerKey = Guard.AgainstNullOrWhiteSpace(request.ProviderKey).Trim();
        var eventId = Guard.AgainstNullOrWhiteSpace(request.EventId).Trim();
        var now = DateTime.UtcNow;

        var existing = await dbContext.WebhookDeliveries
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.ProviderKey == providerKey && x.EventId == eventId, cancellationToken);
        if (existing is not null)
        {
            return new WebhookValidationResultDto(existing.Id, existing.Status != IntegrationWebhookDeliveryStatuses.Rejected, true, existing.Status, existing.FailureReason);
        }

        var connection = await dbContext.IntegrationConnections
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.ProviderKey == providerKey, cancellationToken)
            ?? throw new NotFoundAppException("Integration connection not found.");

        if (!connection.IsEnabled)
        {
            throw new ForbiddenAppException("Integration connection is disabled.");
        }

        var secret = ReadWebhookSecret(connection.SettingsJson);
        var signatureHash = webhookSecurityService.ComputeSignatureHash(request.Signature);
        var payloadHash = webhookSecurityService.ComputePayloadHash(request.PayloadJson);
        var delivery = new IntegrationWebhookDelivery(tenantId, providerKey, eventId, signatureHash, payloadHash, now);
        await dbContext.WebhookDeliveries.AddAsync(delivery, cancellationToken);

        var isValid = webhookSecurityService.ValidateSignature(
            secret,
            request.PayloadJson,
            request.Timestamp,
            request.Signature,
            now,
            TimeSpan.FromMinutes(5));

        if (isValid)
        {
            delivery.MarkProcessed(now);
        }
        else
        {
            delivery.MarkRejected("Webhook signature validation failed.", now);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return new WebhookValidationResultDto(delivery.Id, isValid, false, delivery.Status, delivery.FailureReason);
    }

    private static string ReadWebhookSecret(string settingsJson)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(settingsJson) ? "{}" : settingsJson);
        if (!document.RootElement.TryGetProperty("webhookSecret", out var secretElement) ||
            secretElement.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(secretElement.GetString()))
        {
            throw new ValidationAppException("Integration connection settings must include a webhookSecret value before webhook deliveries can be accepted.");
        }

        return secretElement.GetString()!;
    }
}
