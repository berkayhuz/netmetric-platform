// <copyright file="UpsertProductBundleCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.QuoteManagement.Application.Common;
using NetMetric.CRM.QuoteManagement.Contracts.DTOs;

namespace NetMetric.CRM.QuoteManagement.Application.Commands.Quotes;

public sealed record UpsertProductBundleCommand(
    Guid? ProductBundleId,
    string Code,
    string Name,
    string? Description,
    string? Segment,
    string? Industry,
    decimal DiscountRate,
    decimal? MinimumBudget,
    IReadOnlyList<ProductBundleLineInput> Items,
    string? RowVersion) : IRequest<ProductBundleDto>;
