// <copyright file="EvaluateRulesCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;

namespace NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Commands.EvaluateRules;

public sealed class EvaluateRulesCommandValidator : AbstractValidator<EvaluateRulesCommand>
{
    public EvaluateRulesCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TriggerType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PayloadJson).NotEmpty().MaximumLength(100_000);
        RuleFor(x => x.IdempotencyKey).MaximumLength(500);
        RuleFor(x => x.CorrelationId).MaximumLength(200);
        RuleFor(x => x.LoopDepth).InclusiveBetween(0, 25);
    }
}
