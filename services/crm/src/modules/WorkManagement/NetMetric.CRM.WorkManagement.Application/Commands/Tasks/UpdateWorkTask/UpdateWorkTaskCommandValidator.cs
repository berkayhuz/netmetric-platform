using FluentValidation;

namespace NetMetric.CRM.WorkManagement.Application.Commands.Tasks.UpdateWorkTask;

public sealed class UpdateWorkTaskCommandValidator : AbstractValidator<UpdateWorkTaskCommand>
{
    public UpdateWorkTaskCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Priority).InclusiveBetween(1, 5);
    }
}
