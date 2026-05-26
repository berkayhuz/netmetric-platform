// <copyright file="ChangeOpportunityStageCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.OpportunityManagement.Application.Commands;

public sealed record ChangeOpportunityStageCommand(Guid OpportunityId, OpportunityStageType NewStage, string? Note, string? RowVersion) : IRequest;
