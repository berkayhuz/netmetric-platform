// <copyright file="CreateKnowledgeBaseCategoryCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.KnowledgeBaseManagement.Application.Commands.Categories.CreateKnowledgeBaseCategory;

namespace NetMetric.CRM.KnowledgeBaseManagement.Application.Validators;

public sealed class CreateKnowledgeBaseCategoryCommandValidator : AbstractValidator<CreateKnowledgeBaseCategoryCommand>
{
    public CreateKnowledgeBaseCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
