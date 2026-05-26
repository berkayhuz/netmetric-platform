// <copyright file="BulkAssignContactsOwnerCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Bulk;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Bulk.Commands.BulkAssignContactsOwner;

public sealed record BulkAssignContactsOwnerCommand(
    IReadOnlyCollection<Guid> ContactIds,
    Guid? OwnerUserId) : IRequest<BulkOperationResultDto>;
