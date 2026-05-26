// <copyright file="ListDocumentsForEntityQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Documents;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Documents.Queries.ListDocumentsForEntity;

public sealed class ListDocumentsForEntityQuery : IRequest<IReadOnlyList<DocumentReferenceDto>>
{
    public required string EntityName { get; init; }
    public required Guid EntityId { get; init; }
}
