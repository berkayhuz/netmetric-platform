// <copyright file="AddDocumentVersionCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;

namespace NetMetric.CRM.DocumentManagement.Application.Features.Versions.Commands.AddDocumentVersion;

public sealed class AddDocumentVersionCommandValidator : AbstractValidator<AddDocumentVersionCommand>
{
    public AddDocumentVersionCommandValidator()
    {
        RuleFor(x => x.DocumentId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StorageKey).NotEmpty().MaximumLength(200);
    }
}
