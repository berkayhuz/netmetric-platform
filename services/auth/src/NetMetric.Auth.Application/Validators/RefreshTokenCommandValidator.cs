// <copyright file="RefreshTokenCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.Auth.Application.Features.Commands;

namespace NetMetric.Auth.Application.Validators;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
