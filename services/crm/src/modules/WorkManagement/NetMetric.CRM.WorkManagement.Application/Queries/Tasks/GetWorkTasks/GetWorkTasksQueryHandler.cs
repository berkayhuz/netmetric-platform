using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.WorkManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.WorkManagement.Application.Common;
using NetMetric.CRM.WorkManagement.Contracts.DTOs;
using NetMetric.CRM.WorkManagement.Domain.Enums;
using NetMetric.Pagination;

namespace NetMetric.CRM.WorkManagement.Application.Queries.Tasks.GetWorkTasks;

public sealed class GetWorkTasksQueryHandler(IWorkManagementDbContext dbContext) : IRequestHandler<GetWorkTasksQuery, PagedResult<WorkTaskDto>>
{
    public async Task<PagedResult<WorkTaskDto>> Handle(GetWorkTasksQuery request, CancellationToken cancellationToken)
    {
        var page = PageRequest.Normalize(request.Page, request.PageSize);
        var query = dbContext.Tasks.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.Title.Contains(search) || x.Description.Contains(search));
        }

        if (request.OwnerUserId.HasValue)
        {
            query = query.Where(x => x.OwnerUserId == request.OwnerUserId.Value);
        }

        if (request.DueFromUtc.HasValue)
        {
            query = query.Where(x => x.DueAtUtc >= request.DueFromUtc.Value);
        }

        if (request.DueToUtc.HasValue)
        {
            query = query.Where(x => x.DueAtUtc <= request.DueToUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<WorkItemStatus>(request.Status, true, out var parsedStatus))
        {
            query = query.Where(x => x.Status == parsedStatus);
        }

        var sortBy = request.SortBy?.Trim().ToLowerInvariant();
        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        query = (sortBy, descending) switch
        {
            ("title", false) => query.OrderBy(x => x.Title),
            ("title", true) => query.OrderByDescending(x => x.Title),
            ("priority", false) => query.OrderBy(x => x.Priority),
            ("priority", true) => query.OrderByDescending(x => x.Priority),
            ("status", false) => query.OrderBy(x => x.Status),
            ("status", true) => query.OrderByDescending(x => x.Status),
            ("createdat", false) => query.OrderBy(x => x.CreatedAt),
            ("createdat", true) => query.OrderByDescending(x => x.CreatedAt),
            ("dueatutc", true) => query.OrderByDescending(x => x.DueAtUtc),
            _ => query.OrderBy(x => x.DueAtUtc)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip(page.Skip).Take(page.PageSize).ToListAsync(cancellationToken);

        return PagedResult<WorkTaskDto>.Create(items.Select(x => x.ToDto()).ToList(), totalCount, page);
    }
}
