// <copyright file="ListIntegrationDeadLettersQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.IntegrationHub.Application.DTOs;
using NetMetric.Pagination;

namespace NetMetric.CRM.IntegrationHub.Application.Queries.ListIntegrationDeadLetters;

public sealed record ListIntegrationDeadLettersQuery(
    Guid TenantId,
    string? ProviderKey,
    int Page,
    int PageSize) : IRequest<PagedResult<IntegrationDeadLetterDto>>;
