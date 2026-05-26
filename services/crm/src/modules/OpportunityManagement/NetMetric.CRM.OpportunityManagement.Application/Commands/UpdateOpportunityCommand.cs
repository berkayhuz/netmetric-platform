// <copyright file="UpdateOpportunityCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.OpportunityManagement.Contracts.DTOs;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.OpportunityManagement.Application.Commands;

public sealed record UpdateOpportunityCommand(Guid OpportunityId, string OpportunityCode, string Name, string? Description, decimal EstimatedAmount, decimal? ExpectedRevenue, decimal Probability, DateTime? EstimatedCloseDate, OpportunityStageType Stage, OpportunityStatusType Status, PriorityType Priority, Guid? LeadId, Guid? CustomerId, Guid? OwnerUserId, string? Notes, string RowVersion) : IRequest<OpportunityDetailDto>;
