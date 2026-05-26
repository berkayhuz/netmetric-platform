using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.WorkManagement.Application.Abstractions.Persistence;

namespace NetMetric.CRM.WorkManagement.Application.Commands.Tasks.DeleteWorkTask;

public sealed class DeleteWorkTaskCommandHandler(IWorkManagementDbContext dbContext) : IRequestHandler<DeleteWorkTaskCommand, bool>
{
    public async Task<bool> Handle(DeleteWorkTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await dbContext.Tasks.FirstOrDefaultAsync(x => x.Id == request.TaskId, cancellationToken);
        if (task is null)
        {
            return false;
        }

        dbContext.Tasks.Remove(task);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
