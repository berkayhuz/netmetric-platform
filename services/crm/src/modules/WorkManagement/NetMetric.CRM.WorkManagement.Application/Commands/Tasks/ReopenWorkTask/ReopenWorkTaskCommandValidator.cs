using FluentValidation;

namespace NetMetric.CRM.WorkManagement.Application.Commands.Tasks.ReopenWorkTask;

public sealed class ReopenWorkTaskCommandValidator : AbstractValidator<ReopenWorkTaskCommand>
{
    public ReopenWorkTaskCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
    }
}
