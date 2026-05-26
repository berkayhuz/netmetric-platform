// <copyright file="BulkAssignOpportunitiesOwnerCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.OpportunityManagement.Application.Features.Bulk.Commands.BulkAssignOpportunitiesOwner;

public sealed record BulkAssignOpportunitiesOwnerCommand(IReadOnlyCollection<Guid> OpportunityIds, Guid? OwnerUserId) : IRequest<int>;
