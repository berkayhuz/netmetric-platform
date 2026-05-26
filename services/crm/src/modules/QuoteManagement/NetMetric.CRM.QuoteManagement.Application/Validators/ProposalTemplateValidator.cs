// <copyright file="ProposalTemplateValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;

namespace NetMetric.CRM.QuoteManagement.Application.Validators;

public sealed class ProposalTemplateValidator : AbstractValidator<NetMetric.CRM.QuoteManagement.Application.Commands.ProposalTemplates.CreateProposalTemplateCommand>
{
    public ProposalTemplateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.BodyTemplate).NotEmpty();
    }
}
