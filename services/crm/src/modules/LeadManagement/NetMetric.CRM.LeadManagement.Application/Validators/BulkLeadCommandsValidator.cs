// <copyright file="BulkLeadCommandsValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.LeadManagement.Application.Features.Bulk.Commands.BulkAssignLeadsOwner;
using NetMetric.CRM.LeadManagement.Application.Features.Bulk.Commands.BulkSoftDeleteLeads;

namespace NetMetric.CRM.LeadManagement.Application.Validators;

public sealed class BulkAssignLeadsOwnerCommandValidator : AbstractValidator<BulkAssignLeadsOwnerCommand>
{
    public BulkAssignLeadsOwnerCommandValidator()
    {
        RuleFor(x => x.LeadIds).NotEmpty();
        RuleForEach(x => x.LeadIds).NotEmpty();
    }
}

public sealed class BulkSoftDeleteLeadsCommandValidator : AbstractValidator<BulkSoftDeleteLeadsCommand>
{
    public BulkSoftDeleteLeadsCommandValidator()
    {
        RuleFor(x => x.LeadIds).NotEmpty();
        RuleForEach(x => x.LeadIds).NotEmpty();
    }
}
