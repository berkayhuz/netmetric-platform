// <copyright file="ConvertLeadRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.LeadManagement.Contracts.Requests;

public sealed class ConvertLeadRequest
{
    public CustomerType CustomerType { get; set; } = CustomerType.Individual;
    public bool MarkCustomerAsVip { get; set; }

    public bool CreateCustomer { get; set; } = true;
    public Guid? ExistingCustomerId { get; set; }

    public bool CreateOpportunity { get; set; } = true;
    public string? OpportunityName { get; set; }
    public decimal? EstimatedAmount { get; set; }
    public OpportunityStageType InitialStage { get; set; }
    public PriorityType Priority { get; set; }
    public Guid? OwnerUserId { get; set; }
    public string? Notes { get; set; }

    public Guid? CompanyId { get; set; }
}
