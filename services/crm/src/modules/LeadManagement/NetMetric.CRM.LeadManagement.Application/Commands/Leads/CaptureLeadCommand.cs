// <copyright file="CaptureLeadCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using MediatR;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.LeadManagement.Application.Commands.Leads;

public sealed record CaptureLeadCommand(
    string FullName,
    string? Email,
    string? Phone,
    string? CompanyName,
    string? JobTitle,
    string? Description,
    LeadSourceType Source,
    Guid? CaptureFormId,
    string? ReferrerUrl,
    string? UtmSource,
    string? UtmMedium,
    string? UtmCampaign,
    string? UtmTerm,
    string? UtmContent,
    Dictionary<string, object>? DynamicData) : IRequest<Guid>;
