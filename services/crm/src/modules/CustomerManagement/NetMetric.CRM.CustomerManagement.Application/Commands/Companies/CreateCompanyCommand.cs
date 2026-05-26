// <copyright file="CreateCompanyCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.CustomerManagement.Application.Commands.Companies;

public sealed record CreateCompanyCommand(string Name, string? TaxNumber, string? TaxOffice, string? Website, string? Email, string? Phone, string? Sector, string? EmployeeCountRange, decimal? AnnualRevenue, string? Description, string? Notes, CompanyType CompanyType, Guid? OwnerUserId, Guid? ParentCompanyId) : IRequest<CompanyDetailDto>;
