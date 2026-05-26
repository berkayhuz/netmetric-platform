using FluentValidation;

namespace NetMetric.CRM.WorkManagement.Application.Commands.Tasks.UpdateWorkTaskReminder;

public sealed class UpdateWorkTaskReminderCommandValidator : AbstractValidator<UpdateWorkTaskReminderCommand>
{
    public UpdateWorkTaskReminderCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
    }
}
