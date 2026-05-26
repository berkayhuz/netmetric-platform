// <copyright file="UpdateKnowledgeBaseArticleCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.KnowledgeBaseManagement.Application.Commands.Articles.UpdateKnowledgeBaseArticle;

namespace NetMetric.CRM.KnowledgeBaseManagement.Application.Validators;

public sealed class UpdateKnowledgeBaseArticleCommandValidator : AbstractValidator<UpdateKnowledgeBaseArticleCommand>
{
    public UpdateKnowledgeBaseArticleCommandValidator()
    {
        RuleFor(x => x.ArticleId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Summary).MaximumLength(1000);
        RuleFor(x => x.Content).NotEmpty();
    }
}
