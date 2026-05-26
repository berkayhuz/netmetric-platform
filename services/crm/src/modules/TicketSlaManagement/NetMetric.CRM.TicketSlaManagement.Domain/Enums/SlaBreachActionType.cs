// <copyright file="SlaBreachActionType.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.TicketSlaManagement.Domain.Enums;

public enum SlaBreachActionType
{
    None = 0,
    NotifyOwner = 1,
    NotifyManager = 2,
    ReassignQueue = 3,
    IncreasePriority = 4,
    EscalateToTeam = 5
}
