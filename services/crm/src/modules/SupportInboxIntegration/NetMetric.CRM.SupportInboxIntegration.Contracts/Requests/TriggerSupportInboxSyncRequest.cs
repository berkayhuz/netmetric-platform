// <copyright file="TriggerSupportInboxSyncRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.SupportInboxIntegration.Contracts.Requests;

public sealed class TriggerSupportInboxSyncRequest
{
    public Guid ConnectionId { get; set; }
    public bool DryRun { get; set; }
}
