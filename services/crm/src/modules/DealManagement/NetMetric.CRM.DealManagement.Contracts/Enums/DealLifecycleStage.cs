// <copyright file="DealLifecycleStage.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.DealManagement.Contracts.Enums;

public enum DealLifecycleStage
{
    Open = 1,
    Negotiation = 2,
    Contracting = 3,
    Won = 4,
    Lost = 5,
    Archived = 6
}
