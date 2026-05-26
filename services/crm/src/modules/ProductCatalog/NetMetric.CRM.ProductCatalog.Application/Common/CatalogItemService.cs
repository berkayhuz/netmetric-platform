// <copyright file="CatalogItemService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.ProductCatalog.Application.Abstractions.Persistence;
using NetMetric.CRM.ProductCatalog.Contracts.DTOs;
using NetMetric.CRM.ProductCatalog.Domain.Common;
using NetMetric.CRM.ProductCatalog.Domain.Entities.Categories;
using NetMetric.CRM.ProductCatalog.Domain.Entities.DiscountMatrices;
using NetMetric.CRM.ProductCatalog.Domain.Entities.PriceLists;
using NetMetric.CRM.ProductCatalog.Domain.Entities.ProductBindings;
using NetMetric.CRM.ProductCatalog.Domain.Entities.Products;
using NetMetric.CurrentUser;
using NetMetric.Entities;
using NetMetric.Exceptions;
using NetMetric.Pagination;

namespace NetMetric.CRM.ProductCatalog.Application.Common;

public sealed class CatalogItemService(
    IProductCatalogDbContext dbContext,
    ICurrentUserService currentUserService)
{
    private const int MaxBulkOperationItems = 500;
    private static readonly string[] SupportedCurrencyCodes = ["USD", "EUR", "TRY", "GBP"];

    private readonly IProductCatalogDbContext _dbContext = dbContext;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<PagedResult<ProductCatalogSummaryDto>> ListAsync(
        CatalogEntityKind kind,
        string? search,
        string? code,
        string? name,
        bool? isActive,
        bool includeDeleted,
        int? page,
        int? pageSize,
        string? sortBy,
        string? sortDirection,
        Guid? categoryId,
        CancellationToken cancellationToken)
    {
        var pageRequest = PageRequest.Normalize(page, pageSize);

        return kind switch
        {
            CatalogEntityKind.Products => await ListProductsAsync(search, code, name, isActive, categoryId, includeDeleted, pageRequest, sortBy, sortDirection, cancellationToken),
            CatalogEntityKind.Categories => await ListAsync<CatalogCategory>(search, code, name, isActive, includeDeleted, pageRequest, sortBy, sortDirection, cancellationToken),
            CatalogEntityKind.PriceLists => await ListAsync<PriceList>(search, code, name, isActive, includeDeleted, pageRequest, sortBy, sortDirection, cancellationToken),
            CatalogEntityKind.DiscountMatrices => await ListAsync<DiscountMatrix>(search, code, name, isActive, includeDeleted, pageRequest, sortBy, sortDirection, cancellationToken),
            CatalogEntityKind.ProductBindings => await ListAsync<ProductBinding>(search, code, name, isActive, includeDeleted, pageRequest, sortBy, sortDirection, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }

    public Task<ProductCatalogSummaryDto?> GetByIdAsync(CatalogEntityKind kind, Guid id, bool includeDeleted, CancellationToken cancellationToken)
        => kind switch
        {
            CatalogEntityKind.Products => GetProductByIdAsync(id, includeDeleted, cancellationToken),
            CatalogEntityKind.Categories => GetByIdAsync<CatalogCategory>(id, includeDeleted, cancellationToken),
            CatalogEntityKind.PriceLists => GetByIdAsync<PriceList>(id, includeDeleted, cancellationToken),
            CatalogEntityKind.DiscountMatrices => GetByIdAsync<DiscountMatrix>(id, includeDeleted, cancellationToken),
            CatalogEntityKind.ProductBindings => GetByIdAsync<ProductBinding>(id, includeDeleted, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };

    public Task<ProductCatalogSummaryDto?> GetByCodeAsync(CatalogEntityKind kind, string code, CancellationToken cancellationToken)
        => kind switch
        {
            CatalogEntityKind.Products => GetProductByCodeAsync(code, cancellationToken),
            CatalogEntityKind.Categories => GetByCodeAsync<CatalogCategory>(code, cancellationToken),
            CatalogEntityKind.PriceLists => GetByCodeAsync<PriceList>(code, cancellationToken),
            CatalogEntityKind.DiscountMatrices => GetByCodeAsync<DiscountMatrix>(code, cancellationToken),
            CatalogEntityKind.ProductBindings => GetByCodeAsync<ProductBinding>(code, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };

    public Task<CatalogExistsDto> ExistsByIdAsync(CatalogEntityKind kind, Guid id, CancellationToken cancellationToken)
        => kind switch
        {
            CatalogEntityKind.Products => ExistsByIdAsync<CatalogProduct>(id, cancellationToken),
            CatalogEntityKind.Categories => ExistsByIdAsync<CatalogCategory>(id, cancellationToken),
            CatalogEntityKind.PriceLists => ExistsByIdAsync<PriceList>(id, cancellationToken),
            CatalogEntityKind.DiscountMatrices => ExistsByIdAsync<DiscountMatrix>(id, cancellationToken),
            CatalogEntityKind.ProductBindings => ExistsByIdAsync<ProductBinding>(id, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };

    public Task<CatalogExistsDto> ExistsByCodeAsync(CatalogEntityKind kind, string code, CancellationToken cancellationToken)
        => kind switch
        {
            CatalogEntityKind.Products => ExistsByCodeAsync<CatalogProduct>(code, cancellationToken),
            CatalogEntityKind.Categories => ExistsByCodeAsync<CatalogCategory>(code, cancellationToken),
            CatalogEntityKind.PriceLists => ExistsByCodeAsync<PriceList>(code, cancellationToken),
            CatalogEntityKind.DiscountMatrices => ExistsByCodeAsync<DiscountMatrix>(code, cancellationToken),
            CatalogEntityKind.ProductBindings => ExistsByCodeAsync<ProductBinding>(code, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };

    public Task<ProductCatalogSummaryDto> CreateAsync(
        CatalogEntityKind kind,
        string code,
        string name,
        string? description,
        Guid? categoryId,
        decimal? unitPrice,
        string? currencyCode,
        decimal defaultDiscountRate,
        decimal defaultTaxRate,
        CancellationToken cancellationToken)
        => kind switch
        {
            CatalogEntityKind.Products => CreateProductAsync(code, name, description, categoryId, unitPrice, currencyCode, defaultDiscountRate, defaultTaxRate, cancellationToken),
            CatalogEntityKind.Categories => CreateAsync(code, name, description, static (x, y, z) => new CatalogCategory(x, y, z), "catalog category", cancellationToken),
            CatalogEntityKind.PriceLists => CreateAsync(code, name, description, static (x, y, z) => new PriceList(x, y, z), "price list", cancellationToken),
            CatalogEntityKind.DiscountMatrices => CreateAsync(code, name, description, static (x, y, z) => new DiscountMatrix(x, y, z), "discount matrix", cancellationToken),
            CatalogEntityKind.ProductBindings => CreateAsync(code, name, description, static (x, y, z) => new ProductBinding(x, y, z), "product binding", cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };

    public Task<ProductCatalogSummaryDto> UpdateAsync(
        CatalogEntityKind kind,
        Guid id,
        string code,
        string name,
        string? description,
        Guid? categoryId,
        decimal? unitPrice,
        string? currencyCode,
        decimal defaultDiscountRate,
        decimal defaultTaxRate,
        CancellationToken cancellationToken)
        => kind switch
        {
            CatalogEntityKind.Products => UpdateProductAsync(id, code, name, description, categoryId, unitPrice, currencyCode, defaultDiscountRate, defaultTaxRate, cancellationToken),
            CatalogEntityKind.Categories => UpdateAsync<CatalogCategory>(id, code, name, description, "catalog category", cancellationToken),
            CatalogEntityKind.PriceLists => UpdateAsync<PriceList>(id, code, name, description, "price list", cancellationToken),
            CatalogEntityKind.DiscountMatrices => UpdateAsync<DiscountMatrix>(id, code, name, description, "discount matrix", cancellationToken),
            CatalogEntityKind.ProductBindings => UpdateAsync<ProductBinding>(id, code, name, description, "product binding", cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };

    public Task<ProductCatalogSummaryDto> PatchAsync(
        CatalogEntityKind kind,
        Guid id,
        string? code,
        string? name,
        string? description,
        bool? isActive,
        Guid? categoryId,
        decimal? unitPrice,
        string? currencyCode,
        decimal? defaultDiscountRate,
        decimal? defaultTaxRate,
        CancellationToken cancellationToken)
        => kind switch
        {
            CatalogEntityKind.Products => PatchProductAsync(id, code, name, description, isActive, categoryId, unitPrice, currencyCode, defaultDiscountRate, defaultTaxRate, cancellationToken),
            CatalogEntityKind.Categories => PatchAsync<CatalogCategory>(id, code, name, description, isActive, "catalog category", cancellationToken),
            CatalogEntityKind.PriceLists => PatchAsync<PriceList>(id, code, name, description, isActive, "price list", cancellationToken),
            CatalogEntityKind.DiscountMatrices => PatchAsync<DiscountMatrix>(id, code, name, description, isActive, "discount matrix", cancellationToken),
            CatalogEntityKind.ProductBindings => PatchAsync<ProductBinding>(id, code, name, description, isActive, "product binding", cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };

    public Task DeleteAsync(CatalogEntityKind kind, Guid id, CancellationToken cancellationToken)
        => kind switch
        {
            CatalogEntityKind.Products => DeleteAsync<CatalogProduct>(id, "catalog product", cancellationToken),
            CatalogEntityKind.Categories => DeleteAsync<CatalogCategory>(id, "catalog category", cancellationToken),
            CatalogEntityKind.PriceLists => DeleteAsync<PriceList>(id, "price list", cancellationToken),
            CatalogEntityKind.DiscountMatrices => DeleteAsync<DiscountMatrix>(id, "discount matrix", cancellationToken),
            CatalogEntityKind.ProductBindings => DeleteAsync<ProductBinding>(id, "product binding", cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };

    public Task<ProductCatalogSummaryDto> SetActiveStateAsync(CatalogEntityKind kind, Guid id, bool isActive, CancellationToken cancellationToken)
        => kind switch
        {
            CatalogEntityKind.Products => SetActiveStateAsync<CatalogProduct>(id, isActive, "catalog product", cancellationToken),
            CatalogEntityKind.Categories => SetActiveStateAsync<CatalogCategory>(id, isActive, "catalog category", cancellationToken),
            CatalogEntityKind.PriceLists => SetActiveStateAsync<PriceList>(id, isActive, "price list", cancellationToken),
            CatalogEntityKind.DiscountMatrices => SetActiveStateAsync<DiscountMatrix>(id, isActive, "discount matrix", cancellationToken),
            CatalogEntityKind.ProductBindings => SetActiveStateAsync<ProductBinding>(id, isActive, "product binding", cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };

    public Task<CatalogBulkOperationResultDto> BulkCreateAsync(CatalogEntityKind kind, IReadOnlyCollection<(string Code, string Name, string? Description)> items, CancellationToken cancellationToken)
    {
        EnsureBulkSize(items.Count);
        return kind switch
        {
            CatalogEntityKind.Products => BulkCreateAsync(items, static (x, y, z) => new CatalogProduct(x, y, z), "catalog products", cancellationToken),
            CatalogEntityKind.Categories => BulkCreateAsync(items, static (x, y, z) => new CatalogCategory(x, y, z), "catalog categories", cancellationToken),
            CatalogEntityKind.PriceLists => BulkCreateAsync(items, static (x, y, z) => new PriceList(x, y, z), "price lists", cancellationToken),
            CatalogEntityKind.DiscountMatrices => BulkCreateAsync(items, static (x, y, z) => new DiscountMatrix(x, y, z), "discount matrices", cancellationToken),
            CatalogEntityKind.ProductBindings => BulkCreateAsync(items, static (x, y, z) => new ProductBinding(x, y, z), "product bindings", cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }

    public Task<CatalogBulkOperationResultDto> BulkUpdateAsync(CatalogEntityKind kind, IReadOnlyCollection<(Guid Id, string Code, string Name, string? Description)> items, CancellationToken cancellationToken)
    {
        EnsureBulkSize(items.Count);
        return kind switch
        {
            CatalogEntityKind.Products => BulkUpdateAsync<CatalogProduct>(items, "catalog products", cancellationToken),
            CatalogEntityKind.Categories => BulkUpdateAsync<CatalogCategory>(items, "catalog categories", cancellationToken),
            CatalogEntityKind.PriceLists => BulkUpdateAsync<PriceList>(items, "price lists", cancellationToken),
            CatalogEntityKind.DiscountMatrices => BulkUpdateAsync<DiscountMatrix>(items, "discount matrices", cancellationToken),
            CatalogEntityKind.ProductBindings => BulkUpdateAsync<ProductBinding>(items, "product bindings", cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }

    public Task<CatalogBulkOperationResultDto> BulkDeleteAsync(CatalogEntityKind kind, IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        EnsureBulkSize(ids.Count);
        return kind switch
        {
            CatalogEntityKind.Products => BulkDeleteAsync<CatalogProduct>(ids, cancellationToken),
            CatalogEntityKind.Categories => BulkDeleteAsync<CatalogCategory>(ids, cancellationToken),
            CatalogEntityKind.PriceLists => BulkDeleteAsync<PriceList>(ids, cancellationToken),
            CatalogEntityKind.DiscountMatrices => BulkDeleteAsync<DiscountMatrix>(ids, cancellationToken),
            CatalogEntityKind.ProductBindings => BulkDeleteAsync<ProductBinding>(ids, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }

    public Task<CatalogBulkOperationResultDto> BulkSetActiveStateAsync(CatalogEntityKind kind, IReadOnlyCollection<Guid> ids, bool isActive, CancellationToken cancellationToken)
    {
        EnsureBulkSize(ids.Count);
        return kind switch
        {
            CatalogEntityKind.Products => BulkSetActiveStateAsync<CatalogProduct>(ids, isActive, cancellationToken),
            CatalogEntityKind.Categories => BulkSetActiveStateAsync<CatalogCategory>(ids, isActive, cancellationToken),
            CatalogEntityKind.PriceLists => BulkSetActiveStateAsync<PriceList>(ids, isActive, cancellationToken),
            CatalogEntityKind.DiscountMatrices => BulkSetActiveStateAsync<DiscountMatrix>(ids, isActive, cancellationToken),
            CatalogEntityKind.ProductBindings => BulkSetActiveStateAsync<ProductBinding>(ids, isActive, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }

    public async Task<CatalogImportResultDto> ImportAsync(CatalogEntityKind kind, string csvContent, bool upsertExisting, CancellationToken cancellationToken)
    {
        var rows = ParseImportRows(csvContent);
        var created = 0;
        var updated = 0;

        foreach (var row in rows)
        {
            var existing = await GetByCodeAsync(kind, row.Code, cancellationToken);
            if (existing is null)
            {
                await CreateAsync(kind, row.Code, row.Name, row.Description, null, null, null, 0, 0, cancellationToken);
                created++;
                continue;
            }

            if (!upsertExisting)
                continue;

            await UpdateAsync(kind, existing.Id, row.Code, row.Name, row.Description, null, null, null, 0, 0, cancellationToken);
            updated++;
        }

        return new CatalogImportResultDto
        {
            RequestedCount = rows.Count,
            CreatedCount = created,
            UpdatedCount = updated,
            SkippedCount = rows.Count - created - updated
        };
    }

    public async Task<ExportFileDto> ExportAsync(CatalogEntityKind kind, string? search, string? code, string? name, bool? isActive, CancellationToken cancellationToken)
    {
        var items = await ListAsync(kind, search, code, name, isActive, includeDeleted: false, page: 1, pageSize: 200, sortBy: "code", sortDirection: "asc", categoryId: null, cancellationToken);

        var content = CsvExportBuilder.Build(
            headers: ["Id", "Code", "Name", "Description", "IsActive"],
            rows: items.Items,
            selector: x => [x.Id.ToString(), x.Code, x.Name, x.Description, x.IsActive.ToString()]);

        return new ExportFileDto
        {
            FileName = $"{GetResourceName(kind)}-{DateTime.UtcNow:yyyyMMddHHmmss}.csv",
            ContentType = "text/csv",
            Content = content
        };
    }

    public static ExportFileDto ExportTemplate(CatalogEntityKind kind)
    {
        var content = CsvExportBuilder.Build(
            headers: ["Code", "Name", "Description"],
            rows: new[] { new { Code = $"{GetResourceName(kind)}-001", Name = "Sample Name", Description = "Optional description" } },
            selector: x => [x.Code, x.Name, x.Description]);

        return new ExportFileDto
        {
            FileName = $"{GetResourceName(kind)}-template.csv",
            ContentType = "text/csv",
            Content = content
        };
    }

    public async Task<ProductCatalogStatsDto> GetStatsAsync(CancellationToken cancellationToken)
    {
        var products = await GetStatsAsync<CatalogProduct>(cancellationToken);
        var categories = await GetStatsAsync<CatalogCategory>(cancellationToken);
        var priceLists = await GetStatsAsync<PriceList>(cancellationToken);
        var discountMatrices = await GetStatsAsync<DiscountMatrix>(cancellationToken);
        var productBindings = await GetStatsAsync<ProductBinding>(cancellationToken);

        return new ProductCatalogStatsDto
        {
            ProductCount = products.Total,
            ActiveProductCount = products.Active,
            CategoryCount = categories.Total,
            ActiveCategoryCount = categories.Active,
            PriceListCount = priceLists.Total,
            ActivePriceListCount = priceLists.Active,
            DiscountMatrixCount = discountMatrices.Total,
            ActiveDiscountMatrixCount = discountMatrices.Active,
            ProductBindingCount = productBindings.Total,
            ActiveProductBindingCount = productBindings.Active
        };
    }

    public async Task<ProductCatalogLookupsDto> GetLookupsAsync(CancellationToken cancellationToken)
    {
        return new ProductCatalogLookupsDto
        {
            Products = await GetLookupsAsync<CatalogProduct>(cancellationToken),
            Categories = await GetLookupsAsync<CatalogCategory>(cancellationToken),
            PriceLists = await GetLookupsAsync<PriceList>(cancellationToken),
            DiscountMatrices = await GetLookupsAsync<DiscountMatrix>(cancellationToken),
            ProductBindings = await GetLookupsAsync<ProductBinding>(cancellationToken),
            Currencies = SupportedCurrencyCodes
        };
    }

    public static ProductCatalogMetaDto GetMeta()
    {
        return new ProductCatalogMetaDto
        {
            Module = "ProductCatalog",
            Version = typeof(CatalogItemService).Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Resources = ["products", "categories", "price-lists", "discount-matrices", "product-bindings"],
            Features = ["search", "pagination", "sorting", "bulk-operations", "import-export", "lookups", "stats"]
        };
    }

    private async Task<PagedResult<ProductCatalogSummaryDto>> ListAsync<TEntity>(string? search, string? code, string? name, bool? isActive, bool includeDeleted, PageRequest pageRequest, string? sortBy, string? sortDirection, CancellationToken cancellationToken)
        where TEntity : EntityBase, ICatalogItemEntity
    {
        var query = BuildQuery<TEntity>(includeDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term) || (x.Description != null && x.Description.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(code))
            query = query.Where(x => x.Code == NormalizeCode(code));

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(x => x.Name.Contains(name.Trim()));

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        query = ApplySorting(query, sortBy, sortDirection);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(pageRequest.Skip)
            .Take(pageRequest.Size)
            .Select(x => new ProductCatalogSummaryDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive,
                CurrencyCode = CatalogProduct.DefaultCurrencyCode
            })
            .ToListAsync(cancellationToken);

        return PagedResult<ProductCatalogSummaryDto>.Create(items, totalCount, pageRequest);
    }

    private async Task<PagedResult<ProductCatalogSummaryDto>> ListProductsAsync(string? search, string? code, string? name, bool? isActive, Guid? categoryId, bool includeDeleted, PageRequest pageRequest, string? sortBy, string? sortDirection, CancellationToken cancellationToken)
    {
        var query = BuildQuery<CatalogProduct>(includeDeleted);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term) || (x.Description != null && x.Description.Contains(term)));
        }
        if (!string.IsNullOrWhiteSpace(code)) query = query.Where(x => x.Code == NormalizeCode(code));
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name.Trim()));
        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
        if (categoryId.HasValue) query = query.Where(x => x.CategoryId == categoryId.Value);
        query = ApplyProductSorting(query, sortBy, sortDirection);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip(pageRequest.Skip).Take(pageRequest.Size).Select(x => new ProductCatalogSummaryDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Description = x.Description,
            IsActive = x.IsActive,
            CategoryId = x.CategoryId,
            CategoryCode = x.Category != null ? x.Category.Code : null,
            CategoryName = x.Category != null ? x.Category.Name : null,
            UnitPrice = x.UnitPrice,
            CurrencyCode = x.CurrencyCode,
            PrimaryImageUrl = x.PrimaryImageUrl
        }).ToListAsync(cancellationToken);
        return PagedResult<ProductCatalogSummaryDto>.Create(items, totalCount, pageRequest);
    }

    private async Task<ProductCatalogSummaryDto?> GetProductByIdAsync(Guid id, bool includeDeleted, CancellationToken cancellationToken)
        => await BuildQuery<CatalogProduct>(includeDeleted).Where(x => x.Id == id).Select(x => new ProductCatalogSummaryDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Description = x.Description,
            IsActive = x.IsActive,
            CategoryId = x.CategoryId,
            CategoryCode = x.Category != null ? x.Category.Code : null,
            CategoryName = x.Category != null ? x.Category.Name : null,
            UnitPrice = x.UnitPrice,
            CurrencyCode = x.CurrencyCode,
            PrimaryImageUrl = x.PrimaryImageUrl
        }).SingleOrDefaultAsync(cancellationToken);

    private async Task<ProductCatalogSummaryDto?> GetProductByCodeAsync(string code, CancellationToken cancellationToken)
        => await BuildQuery<CatalogProduct>(includeDeleted: false).Where(x => x.Code == NormalizeCode(code)).Select(x => new ProductCatalogSummaryDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Description = x.Description,
            IsActive = x.IsActive,
            CategoryId = x.CategoryId,
            CategoryCode = x.Category != null ? x.Category.Code : null,
            CategoryName = x.Category != null ? x.Category.Name : null,
            UnitPrice = x.UnitPrice,
            CurrencyCode = x.CurrencyCode,
            PrimaryImageUrl = x.PrimaryImageUrl
        }).SingleOrDefaultAsync(cancellationToken);

    private async Task<ProductCatalogSummaryDto?> GetByIdAsync<TEntity>(Guid id, bool includeDeleted, CancellationToken cancellationToken)
        where TEntity : EntityBase, ICatalogItemEntity
        => await BuildQuery<TEntity>(includeDeleted)
            .Where(x => x.Id == id)
            .Select(x => new ProductCatalogSummaryDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive,
                CurrencyCode = CatalogProduct.DefaultCurrencyCode
            })
            .SingleOrDefaultAsync(cancellationToken);

    private async Task<ProductCatalogSummaryDto?> GetByCodeAsync<TEntity>(string code, CancellationToken cancellationToken)
        where TEntity : EntityBase, ICatalogItemEntity
        => await BuildQuery<TEntity>(includeDeleted: false)
            .Where(x => x.Code == NormalizeCode(code))
            .Select(x => new ProductCatalogSummaryDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive,
                CurrencyCode = CatalogProduct.DefaultCurrencyCode
            })
            .SingleOrDefaultAsync(cancellationToken);

    private async Task<CatalogExistsDto> ExistsByIdAsync<TEntity>(Guid id, CancellationToken cancellationToken)
        where TEntity : EntityBase, ICatalogItemEntity
        => new() { Exists = await BuildQuery<TEntity>(includeDeleted: false).AnyAsync(x => x.Id == id, cancellationToken) };

    private async Task<CatalogExistsDto> ExistsByCodeAsync<TEntity>(string code, CancellationToken cancellationToken)
        where TEntity : EntityBase, ICatalogItemEntity
        => new() { Exists = await BuildQuery<TEntity>(includeDeleted: false).AnyAsync(x => x.Code == NormalizeCode(code), cancellationToken) };

    private async Task<ProductCatalogSummaryDto> CreateAsync<TEntity>(string code, string name, string? description, Func<string, string, string?, TEntity> factory, string entityDisplayName, CancellationToken cancellationToken)
        where TEntity : EntityBase, ICatalogItemEntity
    {
        var normalizedCode = NormalizeCode(code);
        if (await BuildQuery<TEntity>(includeDeleted: false).AnyAsync(x => x.Code == normalizedCode, cancellationToken))
            throw new ConflictAppException($"A {entityDisplayName} with the same code already exists.");

        var entity = factory(normalizedCode, name, description);
        await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
        await SaveChangesWithConflictHandling(entityDisplayName, cancellationToken);
        return MapToDto(entity);
    }

    private async Task<ProductCatalogSummaryDto> UpdateAsync<TEntity>(Guid id, string code, string name, string? description, string entityDisplayName, CancellationToken cancellationToken)
        where TEntity : EntityBase, ICatalogItemEntity
    {
        var entity = await GetRequiredEntityAsync<TEntity>(id, entityDisplayName, cancellationToken);
        var normalizedCode = NormalizeCode(code);

        if (await BuildQuery<TEntity>(includeDeleted: false).AnyAsync(x => x.Id != id && x.Code == normalizedCode, cancellationToken))
            throw new ConflictAppException($"A {entityDisplayName} with the same code already exists.");

        entity.Update(normalizedCode, name, description);
        await SaveChangesWithConflictHandling(entityDisplayName, cancellationToken);
        return MapToDto(entity);
    }

    private async Task<ProductCatalogSummaryDto> PatchAsync<TEntity>(Guid id, string? code, string? name, string? description, bool? isActive, string entityDisplayName, CancellationToken cancellationToken)
        where TEntity : EntityBase, ICatalogItemEntity
    {
        var entity = await GetRequiredEntityAsync<TEntity>(id, entityDisplayName, cancellationToken);
        var nextCode = string.IsNullOrWhiteSpace(code) ? entity.Code : NormalizeCode(code);
        var nextName = string.IsNullOrWhiteSpace(name) ? entity.Name : name.Trim();
        var nextDescription = description is null ? entity.Description : string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        if (await BuildQuery<TEntity>(includeDeleted: false).AnyAsync(x => x.Id != id && x.Code == nextCode, cancellationToken))
            throw new ConflictAppException($"A {entityDisplayName} with the same code already exists.");

        entity.Update(nextCode, nextName, nextDescription);

        if (isActive.HasValue)
        {
            if (isActive.Value)
                entity.Activate();
            else
                entity.Deactivate();
        }

        await SaveChangesWithConflictHandling(entityDisplayName, cancellationToken);
        return MapToDto(entity);
    }

    private async Task DeleteAsync<TEntity>(Guid id, string entityDisplayName, CancellationToken cancellationToken)
        where TEntity : EntityBase, ICatalogItemEntity
    {
        var entity = await GetRequiredEntityAsync<TEntity>(id, entityDisplayName, cancellationToken);
        await AddProductTrashItemIfMissingAsync(entity, cancellationToken);
        _dbContext.Set<TEntity>().Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<ProductCatalogSummaryDto> SetActiveStateAsync<TEntity>(Guid id, bool isActive, string entityDisplayName, CancellationToken cancellationToken)
        where TEntity : EntityBase, ICatalogItemEntity
    {
        var entity = await GetRequiredEntityAsync<TEntity>(id, entityDisplayName, cancellationToken);

        if (isActive)
            entity.Activate();
        else
            entity.Deactivate();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    private async Task<CatalogBulkOperationResultDto> BulkCreateAsync<TEntity>(IReadOnlyCollection<(string Code, string Name, string? Description)> items, Func<string, string, string?, TEntity> factory, string entityDisplayName, CancellationToken cancellationToken)
        where TEntity : EntityBase, ICatalogItemEntity
    {
        var normalizedItems = items.Select(x => (Code: NormalizeCode(x.Code), Name: x.Name, x.Description)).ToList();
        EnsureNoDuplicateCodes(normalizedItems.Select(x => x.Code), entityDisplayName);

        var requestedCodes = normalizedItems.Select(x => x.Code).ToArray();
        var existingCodes = await BuildQuery<TEntity>(includeDeleted: false)
            .Where(x => requestedCodes.Contains(x.Code))
            .Select(x => x.Code)
            .ToListAsync(cancellationToken);

        if (existingCodes.Count != 0)
            throw new ConflictAppException($"Some {entityDisplayName} already exist with the same code.");

        foreach (var item in normalizedItems)
            await _dbContext.Set<TEntity>().AddAsync(factory(item.Code, item.Name, item.Description), cancellationToken);

        await SaveChangesWithConflictHandling(entityDisplayName, cancellationToken);

        return new CatalogBulkOperationResultDto
        {
            RequestedCount = items.Count,
            ProcessedCount = items.Count
        };
    }

    private static void EnsureBulkSize(int count)
    {
        if (count is < 1 or > MaxBulkOperationItems)
        {
            throw new ValidationAppException($"Bulk operations must include between 1 and {MaxBulkOperationItems} items.");
        }
    }

    private async Task<CatalogBulkOperationResultDto> BulkUpdateAsync<TEntity>(IReadOnlyCollection<(Guid Id, string Code, string Name, string? Description)> items, string entityDisplayName, CancellationToken cancellationToken)
        where TEntity : EntityBase, ICatalogItemEntity
    {
        var normalizedItems = items.Select(x => (x.Id, Code: NormalizeCode(x.Code), x.Name, x.Description)).ToList();
        EnsureNoDuplicateCodes(normalizedItems.Select(x => x.Code), entityDisplayName);

        var ids = normalizedItems.Select(x => x.Id).ToArray();
        var entities = await _dbContext.Set<TEntity>()
            .Where(x => x.TenantId == _currentUserService.TenantId && !x.IsDeleted && ids.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (entities.Count != normalizedItems.Count)
            throw new NotFoundAppException($"One or more {entityDisplayName} were not found.");

        var requestedCodes = normalizedItems.Select(x => x.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var conflictingCodes = await BuildQuery<TEntity>(includeDeleted: false)
            .Where(x => !ids.Contains(x.Id) && requestedCodes.Contains(x.Code))
            .Select(x => x.Code)
            .ToListAsync(cancellationToken);

        if (conflictingCodes.Count != 0)
            throw new ConflictAppException($"Some {entityDisplayName} already exist with the same code.");

        foreach (var entity in entities)
        {
            var item = normalizedItems.Single(x => x.Id == entity.Id);
            entity.Update(item.Code, item.Name, item.Description);
        }

        await SaveChangesWithConflictHandling(entityDisplayName, cancellationToken);

        return new CatalogBulkOperationResultDto
        {
            RequestedCount = items.Count,
            ProcessedCount = entities.Count
        };
    }

    private async Task<CatalogBulkOperationResultDto> BulkDeleteAsync<TEntity>(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
        where TEntity : EntityBase, ICatalogItemEntity
    {
        var entities = await _dbContext.Set<TEntity>()
            .Where(x => x.TenantId == _currentUserService.TenantId && !x.IsDeleted && ids.Contains(x.Id))
            .ToListAsync(cancellationToken);

        foreach (var entity in entities)
        {
            await AddProductTrashItemIfMissingAsync(entity, cancellationToken);
            _dbContext.Set<TEntity>().Remove(entity);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CatalogBulkOperationResultDto
        {
            RequestedCount = ids.Count,
            ProcessedCount = entities.Count
        };
    }

    private async Task<CatalogBulkOperationResultDto> BulkSetActiveStateAsync<TEntity>(IReadOnlyCollection<Guid> ids, bool isActive, CancellationToken cancellationToken)
        where TEntity : EntityBase, ICatalogItemEntity
    {
        var entities = await _dbContext.Set<TEntity>()
            .Where(x => x.TenantId == _currentUserService.TenantId && !x.IsDeleted && ids.Contains(x.Id))
            .ToListAsync(cancellationToken);

        foreach (var entity in entities)
        {
            if (isActive)
                entity.Activate();
            else
                entity.Deactivate();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CatalogBulkOperationResultDto
        {
            RequestedCount = ids.Count,
            ProcessedCount = entities.Count
        };
    }

    private async Task<(int Total, int Active)> GetStatsAsync<TEntity>(CancellationToken cancellationToken)
        where TEntity : EntityBase, ICatalogItemEntity
    {
        var query = BuildQuery<TEntity>(includeDeleted: false);
        var total = await query.CountAsync(cancellationToken);
        var active = await query.CountAsync(x => x.IsActive, cancellationToken);
        return (total, active);
    }

    private async Task<IReadOnlyList<CatalogLookupItemDto>> GetLookupsAsync<TEntity>(CancellationToken cancellationToken)
        where TEntity : EntityBase, ICatalogItemEntity
        => await BuildQuery<TEntity>(includeDeleted: false)
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new CatalogLookupItemDto { Id = x.Id, Code = x.Code, Name = x.Name })
            .ToListAsync(cancellationToken);

    private IQueryable<TEntity> BuildQuery<TEntity>(bool includeDeleted)
        where TEntity : EntityBase, ICatalogItemEntity
    {
        _currentUserService.EnsureAuthenticated();
        var query = _dbContext.Set<TEntity>()
            .AsNoTracking()
            .Where(x => x.TenantId == _currentUserService.TenantId);

        return includeDeleted ? query : query.Where(x => !x.IsDeleted);
    }

    private async Task<TEntity> GetRequiredEntityAsync<TEntity>(Guid id, string entityDisplayName, CancellationToken cancellationToken)
        where TEntity : EntityBase, ICatalogItemEntity
    {
        _currentUserService.EnsureAuthenticated();

        var entity = await _dbContext.Set<TEntity>()
            .SingleOrDefaultAsync(x => x.TenantId == _currentUserService.TenantId && !x.IsDeleted && x.Id == id, cancellationToken);

        return entity ?? throw new NotFoundAppException($"{entityDisplayName} not found.");
    }

    private static ProductCatalogSummaryDto MapToDto<TEntity>(TEntity entity)
        where TEntity : EntityBase, ICatalogItemEntity
        => new()
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CategoryId = entity is CatalogProduct productWithCategory ? productWithCategory.CategoryId : null,
            UnitPrice = entity is CatalogProduct productWithPrice ? productWithPrice.UnitPrice : null,
            CurrencyCode = entity is CatalogProduct productWithCurrency ? productWithCurrency.CurrencyCode : CatalogProduct.DefaultCurrencyCode,
            DefaultDiscountRate = entity is CatalogProduct productWithDiscount ? productWithDiscount.DefaultDiscountRate : 0,
            DefaultTaxRate = entity is CatalogProduct productWithTax ? productWithTax.DefaultTaxRate : 0,
            PrimaryImageUrl = entity is CatalogProduct product ? product.PrimaryImageUrl : null
        };

    private async Task<ProductCatalogSummaryDto> CreateProductAsync(
        string code,
        string name,
        string? description,
        Guid? categoryId,
        decimal? unitPrice,
        string? currencyCode,
        decimal defaultDiscountRate,
        decimal defaultTaxRate,
        CancellationToken cancellationToken)
    {
        await EnsureProductReferencesAsync(
            categoryId,
            unitPrice,
            currencyCode,
            defaultDiscountRate,
            defaultTaxRate,
            cancellationToken);

        return await CreateAsync(
            NormalizeCode(code),
            name,
            description,
            (x, y, z) => new CatalogProduct(
                x,
                y,
                z,
                categoryId,
                unitPrice,
                currencyCode,
                defaultDiscountRate,
                defaultTaxRate),
            "catalog product",
            cancellationToken);
    }

    private async Task<ProductCatalogSummaryDto> UpdateProductAsync(
        Guid id,
        string code,
        string name,
        string? description,
        Guid? categoryId,
        decimal? unitPrice,
        string? currencyCode,
        decimal defaultDiscountRate,
        decimal defaultTaxRate,
        CancellationToken cancellationToken)
    {
        await EnsureProductReferencesAsync(
            categoryId,
            unitPrice,
            currencyCode,
            defaultDiscountRate,
            defaultTaxRate,
            cancellationToken);

        var entity = await GetRequiredEntityAsync<CatalogProduct>(id, "catalog product", cancellationToken);
        var normalizedCode = NormalizeCode(code);

        if (await BuildQuery<CatalogProduct>(includeDeleted: false).AnyAsync(x => x.Id != id && x.Code == normalizedCode, cancellationToken))
            throw new ConflictAppException("A catalog product with the same code already exists.");

        entity.Update(
            normalizedCode,
            name,
            description,
            categoryId,
            unitPrice,
            currencyCode,
            defaultDiscountRate,
            defaultTaxRate);
        await SaveChangesWithConflictHandling("catalog product", cancellationToken);
        return MapToDto(entity);
    }

    private async Task<ProductCatalogSummaryDto> PatchProductAsync(
        Guid id,
        string? code,
        string? name,
        string? description,
        bool? isActive,
        Guid? categoryId,
        decimal? unitPrice,
        string? currencyCode,
        decimal? defaultDiscountRate,
        decimal? defaultTaxRate,
        CancellationToken cancellationToken)
    {
        var entity = await GetRequiredEntityAsync<CatalogProduct>(id, "catalog product", cancellationToken);
        var nextCode = string.IsNullOrWhiteSpace(code) ? entity.Code : NormalizeCode(code);
        var nextName = string.IsNullOrWhiteSpace(name) ? entity.Name : name.Trim();
        var nextDescription = description is null ? entity.Description : string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        var nextCategoryId = categoryId ?? entity.CategoryId;
        var nextUnitPrice = unitPrice ?? entity.UnitPrice;
        var nextCurrencyCode = string.IsNullOrWhiteSpace(currencyCode) ? entity.CurrencyCode : currencyCode;
        var nextDefaultDiscountRate = defaultDiscountRate ?? entity.DefaultDiscountRate;
        var nextDefaultTaxRate = defaultTaxRate ?? entity.DefaultTaxRate;

        await EnsureProductReferencesAsync(
            nextCategoryId,
            nextUnitPrice,
            nextCurrencyCode,
            nextDefaultDiscountRate,
            nextDefaultTaxRate,
            cancellationToken);

        if (await BuildQuery<CatalogProduct>(includeDeleted: false).AnyAsync(x => x.Id != id && x.Code == nextCode, cancellationToken))
            throw new ConflictAppException("A catalog product with the same code already exists.");

        entity.Update(
            nextCode,
            nextName,
            nextDescription,
            nextCategoryId,
            nextUnitPrice,
            nextCurrencyCode,
            nextDefaultDiscountRate,
            nextDefaultTaxRate);

        if (isActive.HasValue)
        {
            if (isActive.Value)
                entity.Activate();
            else
                entity.Deactivate();
        }

        await SaveChangesWithConflictHandling("catalog product", cancellationToken);
        return MapToDto(entity);
    }

    private async Task EnsureProductReferencesAsync(
        Guid? categoryId,
        decimal? unitPrice,
        string? currencyCode,
        decimal defaultDiscountRate,
        decimal defaultTaxRate,
        CancellationToken cancellationToken)
    {
        if (unitPrice.HasValue && unitPrice.Value < 0)
        {
            throw new ValidationAppException("Unit price cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            throw new ValidationAppException("Currency code is required.");
        }

        if (defaultDiscountRate < 0 || defaultDiscountRate > 100)
        {
            throw new ValidationAppException("Default discount rate must be between 0 and 100.");
        }

        if (defaultTaxRate < 0 || defaultTaxRate > 100)
        {
            throw new ValidationAppException("Default tax rate must be between 0 and 100.");
        }

        var normalizedCurrency = currencyCode.Trim().ToUpperInvariant();
        if (!SupportedCurrencyCodes.Contains(normalizedCurrency, StringComparer.OrdinalIgnoreCase))
        {
            throw new ValidationAppException("Unsupported currency code.");
        }

        if (!categoryId.HasValue)
        {
            return;
        }

        var categoryExists = await BuildQuery<CatalogCategory>(includeDeleted: false)
            .AnyAsync(x => x.Id == categoryId.Value, cancellationToken);

        if (!categoryExists)
        {
            throw new ValidationAppException("Category was not found.");
        }
    }

    private static IQueryable<TEntity> ApplySorting<TEntity>(IQueryable<TEntity> query, string? sortBy, string? sortDirection)
        where TEntity : EntityBase, ICatalogItemEntity
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return (sortBy ?? "code").Trim().ToLowerInvariant() switch
        {
            "name" => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "createdat" => descending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            "updatedat" => descending ? query.OrderByDescending(x => x.UpdatedAt) : query.OrderBy(x => x.UpdatedAt),
            _ => descending ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };
    }

    private static IQueryable<CatalogProduct> ApplyProductSorting(IQueryable<CatalogProduct> query, string? sortBy, string? sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return (sortBy ?? "code").Trim().ToLowerInvariant() switch
        {
            "categoryname" => descending ? query.OrderByDescending(x => x.Category != null ? x.Category.Name : string.Empty) : query.OrderBy(x => x.Category != null ? x.Category.Name : string.Empty),
            "isactive" => descending ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
            "unitprice" => descending ? query.OrderByDescending(x => x.UnitPrice) : query.OrderBy(x => x.UnitPrice),
            _ => ApplySorting(query, sortBy, sortDirection)
        };
    }

    private async Task SaveChangesWithConflictHandling(string entityDisplayName, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueCodeViolation(ex))
        {
            throw new ConflictAppException($"A {entityDisplayName} with the same code already exists.");
        }
    }

    private static bool IsUniqueCodeViolation(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message;
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        if (!message.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return message.Contains("IX_CatalogProduct_TenantId_Code", StringComparison.OrdinalIgnoreCase)
            || message.Contains("IX_CatalogCategory_TenantId_Code", StringComparison.OrdinalIgnoreCase)
            || message.Contains("IX_PriceList_TenantId_Code", StringComparison.OrdinalIgnoreCase)
            || message.Contains("IX_DiscountMatrix_TenantId_Code", StringComparison.OrdinalIgnoreCase)
            || message.Contains("IX_ProductBinding_TenantId_Code", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeCode(string code) => code.Trim();

    private static void EnsureNoDuplicateCodes(IEnumerable<string> codes, string entityDisplayName)
    {
        if (codes.GroupBy(x => x, StringComparer.OrdinalIgnoreCase).Any(x => x.Count() > 1))
            throw new ConflictAppException($"The request contains duplicate codes for {entityDisplayName}.");
    }

    private async Task AddProductTrashItemIfMissingAsync<TEntity>(TEntity entity, CancellationToken cancellationToken)
        where TEntity : EntityBase, ICatalogItemEntity
    {
        if (entity is not CatalogProduct product)
        {
            return;
        }

        var set = _dbContext.Set<GlobalTrashItem>();
        var exists = _dbContext.Set<GlobalTrashItem>().Local.Any(
            x => x.TenantId == product.TenantId
                 && x.EntityType == CrmTrashEntityTypes.ProductCatalogItem
                 && x.EntityId == product.Id
                 && x.Status == CrmTrashStatuses.Active);

        if (!exists)
        {
            exists = await set.AnyAsync(
                x => x.TenantId == product.TenantId
                     && x.EntityType == CrmTrashEntityTypes.ProductCatalogItem
                     && x.EntityId == product.Id
                     && x.Status == CrmTrashStatuses.Active,
                cancellationToken);
        }

        if (exists)
        {
            return;
        }

        var deletedAtUtc = DateTime.UtcNow;
        var displayName = !string.IsNullOrWhiteSpace(product.Name)
            ? product.Name.Trim()
            : !string.IsNullOrWhiteSpace(product.Code)
                ? product.Code.Trim()
                : "Deleted product";
        var summary = string.IsNullOrWhiteSpace(product.Code) ? null : product.Code.Trim();

        await set.AddAsync(
            new GlobalTrashItem
            {
                TenantId = product.TenantId,
                EntityType = CrmTrashEntityTypes.ProductCatalogItem,
                EntityId = product.Id,
                DisplayName = displayName,
                Summary = summary,
                SourceModule = "product-catalog",
                OriginalRoute = $"/product-catalog/{product.Id:D}",
                DeletedAtUtc = deletedAtUtc,
                DeletedByUserId = _currentUserService.UserId == Guid.Empty ? null : _currentUserService.UserId,
                DeletedByDisplayName = _currentUserService.UserName ?? _currentUserService.Email,
                ExpiresAtUtc = deletedAtUtc.AddDays(7),
                Status = CrmTrashStatuses.Active,
                AuditCorrelationId = Activity.Current?.TraceId.ToString()
            },
            cancellationToken);
    }

    private static string GetResourceName(CatalogEntityKind kind)
        => kind switch
        {
            CatalogEntityKind.Products => "products",
            CatalogEntityKind.Categories => "categories",
            CatalogEntityKind.PriceLists => "price-lists",
            CatalogEntityKind.DiscountMatrices => "discount-matrices",
            CatalogEntityKind.ProductBindings => "product-bindings",
            _ => "catalog-items"
        };

    private static IReadOnlyList<(string Code, string Name, string? Description)> ParseImportRows(string csvContent)
    {
        if (string.IsNullOrWhiteSpace(csvContent))
            return [];

        var lines = csvContent
            .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        if (lines.Count == 0)
            return [];

        var rows = new List<(string Code, string Name, string? Description)>();
        var startIndex = IsHeader(lines[0]) ? 1 : 0;

        for (var i = startIndex; i < lines.Count; i++)
        {
            var cells = ParseCsvLine(lines[i]);
            if (cells.Count < 2)
                continue;

            rows.Add((cells[0], cells[1], cells.Count > 2 ? cells[2] : null));
        }

        return rows;
    }

    private static bool IsHeader(string line)
    {
        var cells = ParseCsvLine(line);
        return cells.Count >= 2
               && string.Equals(cells[0], "Code", StringComparison.OrdinalIgnoreCase)
               && string.Equals(cells[1], "Name", StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var builder = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var current = line[i];

            if (current == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    builder.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (current == ',' && !inQuotes)
            {
                values.Add(builder.ToString().Trim());
                builder.Clear();
                continue;
            }

            builder.Append(current);
        }

        values.Add(builder.ToString().Trim());
        return values;
    }
}
