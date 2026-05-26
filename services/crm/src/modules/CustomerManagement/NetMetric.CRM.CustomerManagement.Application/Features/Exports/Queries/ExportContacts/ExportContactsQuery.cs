// <copyright file="ExportContactsQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Exports;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Exports.Queries.ExportContacts;

public sealed record ExportContactsQuery(
    string? Search = null,
    bool? IsActive = null) : IRequest<ExportFileDto>;
