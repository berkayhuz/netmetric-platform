using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.WorkManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.WorkManagement.Application.Common;
using NetMetric.CRM.WorkManagement.Contracts.DTOs;

namespace NetMetric.CRM.WorkManagement.Application.Commands.Tasks.CompleteWorkTask;

public sealed class CompleteWorkTaskCommandHandler(IWorkManagementDbContext dbContext) : IRequestHandler<CompleteWorkTaskCommand, WorkTaskDto?>
{
    public async Task<WorkTaskDto?> Handle(CompleteWorkTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await dbContext.Tasks.FirstOrDefaultAsync(x => x.Id == request.TaskId, cancellationToken);
        if (task is null)
        {
            return null;
        }

        try
        {
            task.MarkCompleted(request.CompletedByUserId, request.CompletionNote);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            throw new ValidationException(exception.Message);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return task.ToDto();
    }
}
