// <copyright file="ValidateWebhookDeliveryCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.IntegrationHub.Application.DTOs;

namespace NetMetric.CRM.IntegrationHub.Application.Commands.ValidateWebhookDelivery;

public sealed record ValidateWebhookDeliveryCommand(
    Guid TenantId,
    string ProviderKey,
    string EventId,
    string Timestamp,
    string Signature,
    string PayloadJson) : IRequest<WebhookValidationResultDto>;
