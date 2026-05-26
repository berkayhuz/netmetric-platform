using FluentValidation;

namespace NetMetric.CRM.WorkManagement.Application.Commands.Tasks.AssignWorkTaskOwner;

public sealed class AssignWorkTaskOwnerCommandValidator : AbstractValidator<AssignWorkTaskOwnerCommand>
{
    public AssignWorkTaskOwnerCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
    }
}
