using FluentValidation;

namespace NetMetric.CRM.WorkManagement.Application.Commands.Tasks.DeleteWorkTask;

public sealed class DeleteWorkTaskCommandValidator : AbstractValidator<DeleteWorkTaskCommand>
{
    public DeleteWorkTaskCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
    }
}
