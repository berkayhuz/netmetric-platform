// <copyright file="UpsertLeadQualificationCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.LeadManagement.Application.Commands.Leads;

public sealed record UpsertLeadQualificationCommand(
    Guid LeadId,
    QualificationFrameworkType FrameworkType,
    string QualificationDataJson) : IRequest;
