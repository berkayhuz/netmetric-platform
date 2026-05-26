// <copyright file="ListExecutionLogsQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.WorkflowAutomation.Contracts.DTOs;
using NetMetric.Pagination;

namespace NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Queries.ListExecutionLogs;

public sealed record ListExecutionLogsQuery(
    Guid TenantId,
    Guid? RuleId,
    string? Status,
    bool? FailedOnly,
    int Page,
    int PageSize) : IRequest<PagedResult<WorkflowExecutionLogListItemDto>>;
