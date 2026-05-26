// <copyright file="BulkSoftDeleteContactsCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Bulk;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Bulk.Commands.BulkSoftDeleteContacts;

public sealed record BulkSoftDeleteContactsCommand(
    IReadOnlyCollection<Guid> ContactIds) : IRequest<BulkOperationResultDto>;
