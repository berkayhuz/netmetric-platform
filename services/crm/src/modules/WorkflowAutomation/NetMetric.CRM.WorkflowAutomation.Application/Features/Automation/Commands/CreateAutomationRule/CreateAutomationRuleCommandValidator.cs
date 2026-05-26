// <copyright file="CreateAutomationRuleCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;

namespace NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Commands.CreateAutomationRule;

public sealed class CreateAutomationRuleCommandValidator : AbstractValidator<CreateAutomationRuleCommand>
{
    public CreateAutomationRuleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TriggerType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TriggerDefinitionJson).NotEmpty().MaximumLength(100_000);
        RuleFor(x => x.ConditionDefinitionJson).NotEmpty().MaximumLength(100_000);
        RuleFor(x => x.ActionDefinitionJson).NotEmpty().MaximumLength(100_000);
        RuleFor(x => x.Priority).InclusiveBetween(0, 1000);
        RuleFor(x => x.MaxAttempts).InclusiveBetween(1, 10);
        RuleFor(x => x.TenantDailyExecutionLimit).InclusiveBetween(1, 100_000);
        RuleFor(x => x.LoopPreventionWindowSeconds).InclusiveBetween(30, 86_400);
        RuleFor(x => x.MaxLoopDepth).InclusiveBetween(1, 25);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.ScheduleCron).MaximumLength(200);
        RuleFor(x => x.TemplateKey).MaximumLength(100);
        RuleFor(x => x.ScheduleIntervalSeconds).GreaterThanOrEqualTo(30).When(x => x.ScheduleIntervalSeconds.HasValue);
    }
}
