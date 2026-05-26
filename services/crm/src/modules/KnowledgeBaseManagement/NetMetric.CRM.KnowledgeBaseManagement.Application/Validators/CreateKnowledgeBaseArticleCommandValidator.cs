// <copyright file="CreateKnowledgeBaseArticleCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.KnowledgeBaseManagement.Application.Commands.Articles.CreateKnowledgeBaseArticle;

namespace NetMetric.CRM.KnowledgeBaseManagement.Application.Validators;

public sealed class CreateKnowledgeBaseArticleCommandValidator : AbstractValidator<CreateKnowledgeBaseArticleCommand>
{
    public CreateKnowledgeBaseArticleCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Summary).MaximumLength(1000);
        RuleFor(x => x.Content).NotEmpty();
    }
}
