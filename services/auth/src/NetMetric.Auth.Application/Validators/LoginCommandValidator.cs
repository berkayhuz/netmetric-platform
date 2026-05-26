// <copyright file="LoginCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.Auth.Application.Features.Commands;

namespace NetMetric.Auth.Application.Validators;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.EmailOrUserName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(256);
        RuleFor(x => x.MfaCode).MaximumLength(32);
        RuleFor(x => x.RecoveryCode).MaximumLength(128);
    }
}
