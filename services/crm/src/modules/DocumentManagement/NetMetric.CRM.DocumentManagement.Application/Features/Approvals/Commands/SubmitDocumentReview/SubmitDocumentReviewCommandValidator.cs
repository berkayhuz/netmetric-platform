// <copyright file="SubmitDocumentReviewCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;

namespace NetMetric.CRM.DocumentManagement.Application.Features.Approvals.Commands.SubmitDocumentReview;

public sealed class SubmitDocumentReviewCommandValidator : AbstractValidator<SubmitDocumentReviewCommand>
{
    public SubmitDocumentReviewCommandValidator()
    {
        RuleFor(x => x.DocumentId).NotEmpty();
        RuleFor(x => x.ReviewType).NotEmpty().MaximumLength(200);
    }
}
