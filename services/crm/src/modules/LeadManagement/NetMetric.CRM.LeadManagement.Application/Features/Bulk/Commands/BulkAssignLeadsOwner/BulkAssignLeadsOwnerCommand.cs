// <copyright file="BulkAssignLeadsOwnerCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.LeadManagement.Application.Features.Bulk.Commands.BulkAssignLeadsOwner;

public sealed record BulkAssignLeadsOwnerCommand(IReadOnlyCollection<Guid> LeadIds, Guid? OwnerUserId) : IRequest<int>;
