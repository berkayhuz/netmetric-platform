// <copyright file="UpsertRelationshipCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;

namespace NetMetric.CRM.CustomerIntelligence.Application.Features.Customer360.Commands.UpsertRelationship;

public sealed class UpsertRelationshipCommandValidator : AbstractValidator<UpsertRelationshipCommand>
{
    public UpsertRelationshipCommandValidator()
    {
        RuleFor(x => x.SourceEntityType).NotEmpty().MaximumLength(64);
        RuleFor(x => x.TargetEntityType).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RelationshipType).NotEmpty().MaximumLength(64);
        RuleFor(x => x.StrengthScore).InclusiveBetween(0m, 1m);
    }
}
