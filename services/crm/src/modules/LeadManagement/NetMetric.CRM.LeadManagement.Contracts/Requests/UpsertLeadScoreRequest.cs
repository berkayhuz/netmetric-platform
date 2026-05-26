// <copyright file="UpsertLeadScoreRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.LeadManagement.Contracts.Requests;

public sealed class UpsertLeadScoreRequest
{
    public decimal Score { get; set; }
    public string? ScoreReason { get; set; }
}
