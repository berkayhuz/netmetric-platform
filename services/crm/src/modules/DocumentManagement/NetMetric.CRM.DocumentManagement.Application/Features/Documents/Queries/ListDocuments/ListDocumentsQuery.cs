// <copyright file="ListDocumentsQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.DocumentManagement.Contracts.DTOs;
using NetMetric.Pagination;

namespace NetMetric.CRM.DocumentManagement.Application.Features.Documents.Queries.ListDocuments;

public sealed record ListDocumentsQuery(int? Page, int? PageSize, string? Search) : IRequest<PagedResult<DocumentMetadataDto>>;
