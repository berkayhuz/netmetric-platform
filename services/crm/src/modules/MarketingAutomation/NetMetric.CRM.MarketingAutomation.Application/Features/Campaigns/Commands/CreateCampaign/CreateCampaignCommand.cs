// <copyright file="CreateCampaignCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.MarketingAutomation.Contracts.DTOs;

namespace NetMetric.CRM.MarketingAutomation.Application.Features.Campaigns.Commands.CreateCampaign;

public sealed record CreateCampaignCommand(
    string Code,
    string Name,
    string? Description) : IRequest<MarketingAutomationSummaryDto>;
