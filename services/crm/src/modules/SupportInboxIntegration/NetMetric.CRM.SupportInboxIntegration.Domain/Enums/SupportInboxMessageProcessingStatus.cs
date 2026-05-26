// <copyright file="SupportInboxMessageProcessingStatus.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.SupportInboxIntegration.Domain.Enums;

public enum SupportInboxMessageProcessingStatus
{
    Received = 1, Matched = 2, TicketCreated = 3, Ignored = 4, Failed = 5
}
