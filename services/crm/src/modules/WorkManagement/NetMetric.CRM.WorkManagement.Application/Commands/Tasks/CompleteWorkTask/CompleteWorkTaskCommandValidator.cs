using FluentValidation;

namespace NetMetric.CRM.WorkManagement.Application.Commands.Tasks.CompleteWorkTask;

public sealed class CompleteWorkTaskCommandValidator : AbstractValidator<CompleteWorkTaskCommand>
{
    public CompleteWorkTaskCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.CompletionNote).MaximumLength(1000);
    }
}
