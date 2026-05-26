// <copyright file="ListIntegrationJobsQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.IntegrationHub.Application.DTOs;
using NetMetric.Pagination;

namespace NetMetric.CRM.IntegrationHub.Application.Queries.ListIntegrationJobs;

public sealed record ListIntegrationJobsQuery(
    Guid TenantId,
    string? Status,
    string? ProviderKey,
    int Page,
    int PageSize) : IRequest<PagedResult<IntegrationJobListItemDto>>;
