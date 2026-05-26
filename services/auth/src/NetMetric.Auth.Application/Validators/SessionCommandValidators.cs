// <copyright file="SessionCommandValidators.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.Auth.Application.Features.Commands;

namespace NetMetric.Auth.Application.Validators;

public sealed class RevokeSessionCommandValidator : AbstractValidator<RevokeSessionCommand>
{
    public RevokeSessionCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CurrentSessionId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.Email).EmailAddress().MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

public sealed class RevokeOtherSessionsCommandValidator : AbstractValidator<RevokeOtherSessionsCommand>
{
    public RevokeOtherSessionsCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CurrentSessionId).NotEmpty();
        RuleFor(x => x.Email).EmailAddress().MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
