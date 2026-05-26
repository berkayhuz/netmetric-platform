// <copyright file="RemoveCompanyLogoCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;

namespace NetMetric.CRM.CustomerManagement.Application.Commands.Companies;

public sealed record RemoveCompanyLogoCommand(Guid CompanyId) : IRequest<CompanyDetailDto>;
