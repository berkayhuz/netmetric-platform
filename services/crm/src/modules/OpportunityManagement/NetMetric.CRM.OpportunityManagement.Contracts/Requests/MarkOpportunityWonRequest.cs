// <copyright file="MarkOpportunityWonRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.OpportunityManagement.Contracts.Requests;

public sealed class MarkOpportunityWonRequest { public string? DealName { get; set; } public DateTime ClosedDate { get; set; } = DateTime.UtcNow; public string? RowVersion { get; set; } }
