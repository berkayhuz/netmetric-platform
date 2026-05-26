// <copyright file="GetCampaignByIdQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.MarketingAutomation.Contracts.DTOs;

namespace NetMetric.CRM.MarketingAutomation.Application.Features.Campaigns.Queries.GetCampaignById;

public sealed record GetCampaignByIdQuery(Guid Id) : IRequest<MarketingAutomationSummaryDto?>;
