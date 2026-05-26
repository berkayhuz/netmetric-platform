using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.WorkManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.WorkManagement.Application.Common;
using NetMetric.CRM.WorkManagement.Contracts.DTOs;

namespace NetMetric.CRM.WorkManagement.Application.Commands.Tasks.UpdateWorkTask;

public sealed class UpdateWorkTaskCommandHandler(IWorkManagementDbContext dbContext) : IRequestHandler<UpdateWorkTaskCommand, WorkTaskDto?>
{
    public async Task<WorkTaskDto?> Handle(UpdateWorkTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await dbContext.Tasks.FirstOrDefaultAsync(x => x.Id == request.TaskId, cancellationToken);
        if (task is null)
        {
            return null;
        }

        try
        {
            task.UpdateDetails(request.Title, request.Description, request.Priority);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            throw new ValidationException(exception.Message);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return task.ToDto();
    }
}
