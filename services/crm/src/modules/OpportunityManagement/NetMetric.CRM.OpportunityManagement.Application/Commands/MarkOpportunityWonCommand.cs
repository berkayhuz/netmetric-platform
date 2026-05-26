// <copyright file="MarkOpportunityWonCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.OpportunityManagement.Application.Commands;

public sealed record MarkOpportunityWonCommand(Guid OpportunityId, string? DealName, DateTime ClosedDate, string? RowVersion) : IRequest<Guid?>;
