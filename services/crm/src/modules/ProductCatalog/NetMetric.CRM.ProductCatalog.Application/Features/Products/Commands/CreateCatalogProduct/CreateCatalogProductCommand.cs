// <copyright file="CreateCatalogProductCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.ProductCatalog.Contracts.DTOs;

namespace NetMetric.CRM.ProductCatalog.Application.Features.Products.Commands.CreateCatalogProduct;

public sealed record CreateCatalogProductCommand(
    string Code,
    string Name,
    string? Description,
    Guid? CategoryId,
    decimal? UnitPrice,
    string CurrencyCode) : IRequest<ProductCatalogSummaryDto>;
