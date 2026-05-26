// <copyright file="LogoutCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.Auth.Application.Features.Commands;

namespace NetMetric.Auth.Application.Validators;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
