// <copyright file="RegisterCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.Extensions.Options;
using NetMetric.Auth.Application.Features.Commands;
using NetMetric.Auth.Application.Options;
using NetMetric.Localization;

namespace NetMetric.Auth.Application.Validators;

public sealed partial class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator(IOptions<PasswordPolicyOptions> passwordPolicyOptions)
    {
        var policy = passwordPolicyOptions.Value;

        RuleFor(x => x.TenantName).NotEmpty().MinimumLength(2).MaximumLength(200);
        RuleFor(x => x.UserName).NotEmpty().MinimumLength(3).MaximumLength(64);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password)
            .ApplyPasswordPolicy(policy);
        RuleFor(x => x.Culture)
            .MaximumLength(20)
            .Must(value => string.IsNullOrWhiteSpace(value) || NetMetricCultures.IsSupportedCulture(value))
            .WithMessage($"Culture must be one of: {string.Join(", ", NetMetricCultures.SupportedCultureNames)}.");
    }
}
