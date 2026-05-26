// <copyright file="ChangeOpportunityStageRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.OpportunityManagement.Contracts.Requests;

public sealed class ChangeOpportunityStageRequest
{
    public OpportunityStageType NewStage { get; set; }
    public string? Note { get; set; }
    public string? RowVersion { get; set; }
}
