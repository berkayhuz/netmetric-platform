// <copyright file="AssignLeadOwnerRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.LeadManagement.Contracts.Requests;

public sealed class AssignLeadOwnerRequest
{
    public Guid? OwnerUserId { get; set; }
}
