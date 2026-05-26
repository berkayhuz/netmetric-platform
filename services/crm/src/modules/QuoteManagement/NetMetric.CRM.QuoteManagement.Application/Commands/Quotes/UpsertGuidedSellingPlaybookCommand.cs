// <copyright file="UpsertGuidedSellingPlaybookCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.QuoteManagement.Contracts.DTOs;

namespace NetMetric.CRM.QuoteManagement.Application.Commands.Quotes;

public sealed record UpsertGuidedSellingPlaybookCommand(
    Guid? GuidedSellingPlaybookId,
    string Name,
    string? Segment,
    string? Industry,
    decimal? MinimumBudget,
    decimal? MaximumBudget,
    string? RequiredCapabilities,
    IReadOnlyList<string> RecommendedBundleCodes,
    string? QualificationJson,
    string? RowVersion) : IRequest<GuidedSellingPlaybookDto>;
