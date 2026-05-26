// <copyright file="ResolveIdentityCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;

namespace NetMetric.CRM.CustomerIntelligence.Application.Features.Cdp.Commands.ResolveIdentity;

public sealed class ResolveIdentityCommandValidator : AbstractValidator<ResolveIdentityCommand>
{
    public ResolveIdentityCommandValidator()
    {
        RuleFor(x => x.SubjectType).NotEmpty().MaximumLength(64);
        RuleFor(x => x.IdentityType).NotEmpty().MaximumLength(64);
        RuleFor(x => x.IdentityValue).NotEmpty().MaximumLength(256);
        RuleFor(x => x.ConfidenceScore).InclusiveBetween(0m, 1m);
    }
}
