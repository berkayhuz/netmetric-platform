// <copyright file="ConvertLeadRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.PipelineManagement.Contracts.Requests;

public sealed record ConvertLeadRequest(bool CreateCustomer, bool CreateOpportunity, Guid? ExistingCustomerId, string? OpportunityName, decimal? EstimatedAmount, OpportunityStageType InitialStage, PriorityType Priority, Guid? OwnerUserId, string? Notes);
