// <copyright file="MarkOpportunityLostRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.OpportunityManagement.Contracts.Requests;

public sealed class MarkOpportunityLostRequest { public Guid? LostReasonId { get; set; } public string? LostNote { get; set; } public string? RowVersion { get; set; } }
