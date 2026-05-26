// <copyright file="ConvertLeadCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.PipelineManagement.Contracts.DTOs;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.PipelineManagement.Application.Commands;

public sealed record ConvertLeadCommand(Guid LeadId, bool CreateCustomer, bool CreateOpportunity, Guid? ExistingCustomerId, string? OpportunityName, decimal? EstimatedAmount, OpportunityStageType InitialStage, PriorityType Priority, Guid? OwnerUserId, string? Notes) : IRequest<LeadConversionResultDto>;
