// <copyright file="BulkAssignOpportunityOwnerRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.OpportunityManagement.Contracts.Requests;

public sealed class BulkAssignOpportunityOwnerRequest { public List<Guid> OpportunityIds { get; set; } = []; public Guid? OwnerUserId { get; set; } }
