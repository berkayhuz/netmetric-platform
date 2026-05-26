// <copyright file="BulkChangeOpportunityStageCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.OpportunityManagement.Application.Features.Bulk.Commands.BulkChangeOpportunityStage;

public sealed record BulkChangeOpportunityStageCommand(IReadOnlyCollection<Guid> OpportunityIds, OpportunityStageType NewStage, string? Note) : IRequest<int>;
