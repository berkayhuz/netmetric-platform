// <copyright file="GetCompanyWorkspaceQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Companies;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Companies.Queries.GetCompanyWorkspace;

public sealed record GetCompanyWorkspaceQuery(Guid CompanyId) : IRequest<CompanyWorkspaceDto>;
