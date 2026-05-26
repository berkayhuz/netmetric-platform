// <copyright file="SupportInboxSyncRunStatus.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.SupportInboxIntegration.Domain.Enums;

public enum SupportInboxSyncRunStatus
{
    Started = 1, Completed = 2, CompletedWithErrors = 3, Failed = 4
}
