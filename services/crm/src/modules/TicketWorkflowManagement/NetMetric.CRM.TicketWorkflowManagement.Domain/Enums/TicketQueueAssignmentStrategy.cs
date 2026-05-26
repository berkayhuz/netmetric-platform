// <copyright file="TicketQueueAssignmentStrategy.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.TicketWorkflowManagement.Domain.Enums;

public enum TicketQueueAssignmentStrategy
{
    Manual = 1,
    RoundRobin = 2,
    LeastLoaded = 3
}
