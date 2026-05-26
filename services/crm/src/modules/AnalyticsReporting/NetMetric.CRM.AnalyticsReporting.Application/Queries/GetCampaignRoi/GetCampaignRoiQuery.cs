// <copyright file="GetCampaignRoiQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.AnalyticsReporting.Application.DTOs;

namespace NetMetric.CRM.AnalyticsReporting.Application.Queries.GetCampaignRoi;

public sealed record GetCampaignRoiQuery(Guid TenantId) : IRequest<IReadOnlyCollection<CampaignRoiDto>>;
