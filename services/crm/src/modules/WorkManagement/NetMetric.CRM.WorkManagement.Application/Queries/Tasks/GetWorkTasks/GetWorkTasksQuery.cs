using MediatR;
using NetMetric.CRM.WorkManagement.Contracts.DTOs;
using NetMetric.Pagination;

namespace NetMetric.CRM.WorkManagement.Application.Queries.Tasks.GetWorkTasks;

public sealed record GetWorkTasksQuery(
    string? Search,
    string? Status,
    Guid? OwnerUserId,
    DateTime? DueFromUtc,
    DateTime? DueToUtc,
    int Page,
    int PageSize,
    string? SortBy,
    string? SortDirection) : IRequest<PagedResult<WorkTaskDto>>;
