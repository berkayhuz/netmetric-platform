// <copyright file="CreateSupportInboxRuleCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.SupportInboxIntegration.Application.Commands.Rules.CreateSupportInboxRule;

namespace NetMetric.CRM.SupportInboxIntegration.Application.Validators;

public sealed class CreateSupportInboxRuleCommandValidator : AbstractValidator<CreateSupportInboxRuleCommand>
{
    public CreateSupportInboxRuleCommandValidator()
    {
        RuleFor(x => x.ConnectionId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
    }
}
