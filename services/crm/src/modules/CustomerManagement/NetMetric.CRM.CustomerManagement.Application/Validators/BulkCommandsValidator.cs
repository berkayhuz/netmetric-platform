// <copyright file="BulkCommandsValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.CustomerManagement.Application.Features.Bulk.Commands.BulkAssignCompaniesOwner;
using NetMetric.CRM.CustomerManagement.Application.Features.Bulk.Commands.BulkAssignContactsOwner;
using NetMetric.CRM.CustomerManagement.Application.Features.Bulk.Commands.BulkAssignCustomersOwner;
using NetMetric.CRM.CustomerManagement.Application.Features.Bulk.Commands.BulkSoftDeleteCompanies;
using NetMetric.CRM.CustomerManagement.Application.Features.Bulk.Commands.BulkSoftDeleteContacts;
using NetMetric.CRM.CustomerManagement.Application.Features.Bulk.Commands.BulkSoftDeleteCustomers;

namespace NetMetric.CRM.CustomerManagement.Application.Validators;

public sealed class BulkAssignCompaniesOwnerCommandValidator : AbstractValidator<BulkAssignCompaniesOwnerCommand>
{
    public BulkAssignCompaniesOwnerCommandValidator()
    {
        RuleFor(x => x.CompanyIds).NotEmpty();
    }
}

public sealed class BulkAssignContactsOwnerCommandValidator : AbstractValidator<BulkAssignContactsOwnerCommand>
{
    public BulkAssignContactsOwnerCommandValidator()
    {
        RuleFor(x => x.ContactIds).NotEmpty();
    }
}

public sealed class BulkAssignCustomersOwnerCommandValidator : AbstractValidator<BulkAssignCustomersOwnerCommand>
{
    public BulkAssignCustomersOwnerCommandValidator()
    {
        RuleFor(x => x.CustomerIds).NotEmpty();
    }
}

public sealed class BulkSoftDeleteCompaniesCommandValidator : AbstractValidator<BulkSoftDeleteCompaniesCommand>
{
    public BulkSoftDeleteCompaniesCommandValidator()
    {
        RuleFor(x => x.CompanyIds).NotEmpty();
    }
}

public sealed class BulkSoftDeleteContactsCommandValidator : AbstractValidator<BulkSoftDeleteContactsCommand>
{
    public BulkSoftDeleteContactsCommandValidator()
    {
        RuleFor(x => x.ContactIds).NotEmpty();
    }
}

public sealed class BulkSoftDeleteCustomersCommandValidator : AbstractValidator<BulkSoftDeleteCustomersCommand>
{
    public BulkSoftDeleteCustomersCommandValidator()
    {
        RuleFor(x => x.CustomerIds).NotEmpty();
    }
}
