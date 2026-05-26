// <copyright file="UpdateTenantBrandingCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.TenantManagement.Application.Commands.UpdateTenantBranding;

public sealed record UpdateTenantBrandingCommand(
    Guid TenantId,
    string? PrimaryDomain,
    string Locale,
    string TimeZone,
    string? BrandPrimaryColor,
    string? LogoUrl) : IRequest;
