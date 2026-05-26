// <copyright file="MergeEntitiesCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerIntelligence.Contracts.DTOs;

namespace NetMetric.CRM.CustomerIntelligence.Application.Features.Merges.Commands.MergeEntities;

public sealed record MergeEntitiesCommand(string PrimaryEntityType, Guid PrimaryEntityId, string SecondaryEntityType, Guid SecondaryEntityId, string Reason) : IRequest<Guid>;
