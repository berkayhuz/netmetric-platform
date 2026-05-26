// <copyright file="BulkAssignCompaniesOwnerCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Bulk;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Bulk.Commands.BulkAssignCompaniesOwner;

public sealed record BulkAssignCompaniesOwnerCommand(
    IReadOnlyCollection<Guid> CompanyIds,
    Guid? OwnerUserId) : IRequest<BulkOperationResultDto>;
