// <copyright file="GetCatalogProductByIdQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.ProductCatalog.Contracts.DTOs;

namespace NetMetric.CRM.ProductCatalog.Application.Features.Products.Queries.GetCatalogProductById;

public sealed record GetCatalogProductByIdQuery(Guid Id) : IRequest<ProductCatalogSummaryDto?>;
