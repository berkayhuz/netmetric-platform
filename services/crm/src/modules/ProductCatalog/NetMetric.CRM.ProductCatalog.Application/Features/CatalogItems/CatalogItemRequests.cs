// <copyright file="CatalogItemRequests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.ProductCatalog.Application.Common;
using NetMetric.CRM.ProductCatalog.Contracts.DTOs;
using NetMetric.Pagination;

namespace NetMetric.CRM.ProductCatalog.Application.Features.CatalogItems;

public sealed record GetCatalogItemsQuery(
    CatalogEntityKind Kind,
    string? Search = null,
    string? Code = null,
    string? Name = null,
    bool? IsActive = null,
    bool IncludeDeleted = false,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = "code",
    string? SortDirection = "asc",
    Guid? CategoryId = null) : IRequest<PagedResult<ProductCatalogSummaryDto>>;

public sealed record GetCatalogItemByIdQuery(CatalogEntityKind Kind, Guid Id, bool IncludeDeleted = false) : IRequest<ProductCatalogSummaryDto?>;
public sealed record GetCatalogItemByCodeQuery(CatalogEntityKind Kind, string Code) : IRequest<ProductCatalogSummaryDto?>;
public sealed record CatalogItemExistsByIdQuery(CatalogEntityKind Kind, Guid Id) : IRequest<CatalogExistsDto>;
public sealed record CatalogItemExistsByCodeQuery(CatalogEntityKind Kind, string Code) : IRequest<CatalogExistsDto>;
public sealed record CreateCatalogItemCommand(
    CatalogEntityKind Kind,
    string Code,
    string Name,
    string? Description,
    Guid? CategoryId = null,
    decimal? UnitPrice = null,
    string? CurrencyCode = null,
    decimal DefaultDiscountRate = 0,
    decimal DefaultTaxRate = 0) : IRequest<ProductCatalogSummaryDto>;
public sealed record UpdateCatalogItemCommand(
    CatalogEntityKind Kind,
    Guid Id,
    string Code,
    string Name,
    string? Description,
    Guid? CategoryId = null,
    decimal? UnitPrice = null,
    string? CurrencyCode = null,
    decimal DefaultDiscountRate = 0,
    decimal DefaultTaxRate = 0) : IRequest<ProductCatalogSummaryDto>;
public sealed record PatchCatalogItemCommand(
    CatalogEntityKind Kind,
    Guid Id,
    string? Code,
    string? Name,
    string? Description,
    bool? IsActive,
    Guid? CategoryId = null,
    decimal? UnitPrice = null,
    string? CurrencyCode = null,
    decimal? DefaultDiscountRate = null,
    decimal? DefaultTaxRate = null) : IRequest<ProductCatalogSummaryDto>;
public sealed record DeleteCatalogItemCommand(CatalogEntityKind Kind, Guid Id) : IRequest;
public sealed record SetCatalogItemActiveStateCommand(CatalogEntityKind Kind, Guid Id, bool IsActive) : IRequest<ProductCatalogSummaryDto>;
public sealed record BulkCreateCatalogItemsCommand(CatalogEntityKind Kind, IReadOnlyCollection<(string Code, string Name, string? Description)> Items) : IRequest<CatalogBulkOperationResultDto>;
public sealed record BulkUpdateCatalogItemsCommand(CatalogEntityKind Kind, IReadOnlyCollection<(Guid Id, string Code, string Name, string? Description)> Items) : IRequest<CatalogBulkOperationResultDto>;
public sealed record BulkDeleteCatalogItemsCommand(CatalogEntityKind Kind, IReadOnlyCollection<Guid> Ids) : IRequest<CatalogBulkOperationResultDto>;
public sealed record BulkSetCatalogItemsActiveStateCommand(CatalogEntityKind Kind, IReadOnlyCollection<Guid> Ids, bool IsActive) : IRequest<CatalogBulkOperationResultDto>;
public sealed record ImportCatalogItemsCommand(CatalogEntityKind Kind, string CsvContent, bool UpsertExisting) : IRequest<CatalogImportResultDto>;
public sealed record ExportCatalogItemsQuery(CatalogEntityKind Kind, string? Search, string? Code, string? Name, bool? IsActive) : IRequest<ExportFileDto>;
public sealed record ExportCatalogItemsTemplateQuery(CatalogEntityKind Kind) : IRequest<ExportFileDto>;
public sealed record GetProductCatalogMetaQuery() : IRequest<ProductCatalogMetaDto>;
public sealed record GetProductCatalogStatsQuery() : IRequest<ProductCatalogStatsDto>;
public sealed record GetProductCatalogLookupsQuery() : IRequest<ProductCatalogLookupsDto>;
