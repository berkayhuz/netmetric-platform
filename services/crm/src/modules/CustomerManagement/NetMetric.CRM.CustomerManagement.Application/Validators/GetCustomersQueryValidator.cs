// <copyright file="GetCustomersQueryValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.CustomerManagement.Application.Queries.Customers;

namespace NetMetric.CRM.CustomerManagement.Application.Validators;

public sealed class GetCustomersQueryValidator : AbstractValidator<GetCustomersQuery>
{
    private static readonly string[] AllowedSortFields =
    [
        "name",
        "fullName",
        "email",
        "mobilePhone",
        "company",
        "companyName",
        "customerType",
        "isActive",
        "createdAt",
        "isVip"
    ];
    private static readonly string[] AllowedSortDirections = ["asc", "desc"];

    public GetCustomersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 500);
        RuleFor(x => x.Search).MaximumLength(200);
        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrWhiteSpace(x) || AllowedSortFields.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Unsupported customer sort field.");
        RuleFor(x => x.SortDirection)
            .Must(x => string.IsNullOrWhiteSpace(x) || AllowedSortDirections.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Sort direction must be asc or desc.");
    }
}
