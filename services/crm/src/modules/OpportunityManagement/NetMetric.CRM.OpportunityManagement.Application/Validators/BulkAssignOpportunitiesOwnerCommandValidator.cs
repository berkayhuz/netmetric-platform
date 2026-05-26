// <copyright file="BulkAssignOpportunitiesOwnerCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.OpportunityManagement.Application.Features.Bulk.Commands.BulkAssignOpportunitiesOwner;

namespace NetMetric.CRM.OpportunityManagement.Application.Validators;

public sealed class BulkAssignOpportunitiesOwnerCommandValidator : AbstractValidator<BulkAssignOpportunitiesOwnerCommand>
{
    public BulkAssignOpportunitiesOwnerCommandValidator() => RuleFor(x => x.OpportunityIds).NotEmpty();
}
