// <copyright file="GetSmartLabelRulesQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TagManagement.Contracts.DTOs;

namespace NetMetric.CRM.TagManagement.Application.Features.SmartLabels.Queries.GetSmartLabelRules;

public sealed record GetSmartLabelRulesQuery : IRequest<IReadOnlyList<SmartLabelRuleSummaryDto>>;
