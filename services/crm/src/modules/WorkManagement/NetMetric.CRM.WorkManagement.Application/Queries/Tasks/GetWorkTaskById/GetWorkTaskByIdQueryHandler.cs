using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.WorkManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.WorkManagement.Application.Common;
using NetMetric.CRM.WorkManagement.Contracts.DTOs;

namespace NetMetric.CRM.WorkManagement.Application.Queries.Tasks.GetWorkTaskById;

public sealed class GetWorkTaskByIdQueryHandler(IWorkManagementDbContext dbContext) : IRequestHandler<GetWorkTaskByIdQuery, WorkTaskDto?>
{
    public async Task<WorkTaskDto?> Handle(GetWorkTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await dbContext.Tasks.FirstOrDefaultAsync(x => x.Id == request.TaskId, cancellationToken);
        return task?.ToDto();
    }
}
