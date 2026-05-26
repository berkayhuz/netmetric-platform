// <copyright file="IntegrationJobProcessingState.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Options;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Processing;

namespace NetMetric.CRM.IntegrationHub.Infrastructure.Processing;

public sealed class IntegrationJobProcessingState(IOptionsMonitor<IntegrationJobProcessingOptions> options) : IIntegrationJobProcessingState
{
    public bool IsEnabled => options.CurrentValue.Enabled;
}
