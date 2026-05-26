// <copyright file="ISupportInboxSynchronizationService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.SupportInboxIntegration.Application.Abstractions.Services;

public interface ISupportInboxSynchronizationService
{
    Task RunAsync(Guid connectionId, bool dryRun, CancellationToken cancellationToken = default);
}
