// <copyright file="RoleManagementCommandValidators.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.Auth.Application.Features.Commands;

namespace NetMetric.Auth.Application.Validators;

public sealed class ListTenantMembersCommandValidator : AbstractValidator<ListTenantMembersCommand>
{
    public ListTenantMembersCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequestedByUserId).NotEmpty();
    }
}

public sealed class UpdateTenantMemberRolesCommandValidator : AbstractValidator<UpdateTenantMemberRolesCommand>
{
    public UpdateTenantMemberRolesCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TargetUserId).NotEmpty();
        RuleFor(x => x.RequestedByUserId).NotEmpty();
        RuleFor(x => x.Roles).Must(x => x is { Count: > 0 }).WithMessage("At least one role is required.");
        RuleForEach(x => x.Roles)
            .NotEmpty()
            .MaximumLength(128);
    }
}

public sealed class ListRoleCatalogCommandValidator : AbstractValidator<ListRoleCatalogCommand>
{
    public ListRoleCatalogCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequestedByUserId).NotEmpty();
    }
}
