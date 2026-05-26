using FluentValidation;

namespace NetMetric.CRM.WorkManagement.Application.Commands.Tasks.UpdateWorkTaskDueDate;

public sealed class UpdateWorkTaskDueDateCommandValidator : AbstractValidator<UpdateWorkTaskDueDateCommand>
{
    public UpdateWorkTaskDueDateCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
    }
}
