// <copyright file="UpsertIntegrationConnectionCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.IntegrationHub.Application.Commands.UpsertIntegrationConnection;

public sealed record UpsertIntegrationConnectionCommand(
    Guid TenantId,
    string ProviderKey,
    string DisplayName,
    string Category,
    string SettingsJson,
    bool IsEnabled) : IRequest<Guid>;
