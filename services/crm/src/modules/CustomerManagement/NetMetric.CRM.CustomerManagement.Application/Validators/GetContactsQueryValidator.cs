// <copyright file="GetContactsQueryValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.CustomerManagement.Application.Queries.Contacts;

namespace NetMetric.CRM.CustomerManagement.Application.Validators;

public sealed class GetContactsQueryValidator : AbstractValidator<GetContactsQuery>
{
    private static readonly string[] AllowedSortFields = ["name", "email", "createdAt", "company", "customer"];
    private static readonly string[] AllowedSortDirections = ["asc", "desc"];

    public GetContactsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
        RuleFor(x => x.Search).MaximumLength(200);
        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrWhiteSpace(x) || AllowedSortFields.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Unsupported contact sort field.");
        RuleFor(x => x.SortDirection)
            .Must(x => string.IsNullOrWhiteSpace(x) || AllowedSortDirections.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Sort direction must be asc or desc.");
    }
}
