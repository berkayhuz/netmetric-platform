using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.WorkManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.WorkManagement.Application.Common;
using NetMetric.CRM.WorkManagement.Contracts.DTOs;

namespace NetMetric.CRM.WorkManagement.Application.Commands.Tasks.AssignWorkTaskOwner;

public sealed class AssignWorkTaskOwnerCommandHandler(IWorkManagementDbContext dbContext) : IRequestHandler<AssignWorkTaskOwnerCommand, WorkTaskDto?>
{
    public async Task<WorkTaskDto?> Handle(AssignWorkTaskOwnerCommand request, CancellationToken cancellationToken)
    {
        var task = await dbContext.Tasks.FirstOrDefaultAsync(x => x.Id == request.TaskId, cancellationToken);
        if (task is null)
        {
            return null;
        }

        try
        {
            task.AssignOwner(request.OwnerUserId);
        }
        catch (Exception exception) when (exception is InvalidOperationException)
        {
            throw new ValidationException(exception.Message);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return task.ToDto();
    }
}
