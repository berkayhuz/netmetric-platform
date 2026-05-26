// <copyright file="UpdateLeadCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.LeadManagement.Contracts.DTOs;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.LeadManagement.Application.Commands.Leads;

public sealed record UpdateLeadCommand(
    Guid LeadId,
    string FullName,
    string? CompanyName,
    string? Email,
    string? Phone,
    string? JobTitle,
    string? Description,
    decimal? EstimatedBudget,
    DateTime? NextContactDate,
    LeadSourceType Source,
    LeadStatusType Status,
    PriorityType Priority,
    Guid? CompanyId,
    Guid? OwnerUserId,
    string? Notes,
    string? RowVersion) : IRequest<LeadDetailDto>;
