// <copyright file="CatalogItemHandlers.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.ProductCatalog.Application.Common;
using NetMetric.CRM.ProductCatalog.Contracts.DTOs;
using NetMetric.Pagination;

namespace NetMetric.CRM.ProductCatalog.Application.Features.CatalogItems;

public sealed class GetCatalogItemsQueryHandler(CatalogItemService service) : IRequestHandler<GetCatalogItemsQuery, PagedResult<ProductCatalogSummaryDto>>
{
    public Task<PagedResult<ProductCatalogSummaryDto>> Handle(GetCatalogItemsQuery request, CancellationToken cancellationToken)
        => service.ListAsync(request.Kind, request.Search, request.Code, request.Name, request.IsActive, request.IncludeDeleted, request.Page, request.PageSize, request.SortBy, request.SortDirection, request.CategoryId, cancellationToken);
}

public sealed class GetCatalogItemByIdQueryHandler(CatalogItemService service) : IRequestHandler<GetCatalogItemByIdQuery, ProductCatalogSummaryDto?>
{
    public Task<ProductCatalogSummaryDto?> Handle(GetCatalogItemByIdQuery request, CancellationToken cancellationToken)
        => service.GetByIdAsync(request.Kind, request.Id, request.IncludeDeleted, cancellationToken);
}

public sealed class GetCatalogItemByCodeQueryHandler(CatalogItemService service) : IRequestHandler<GetCatalogItemByCodeQuery, ProductCatalogSummaryDto?>
{
    public Task<ProductCatalogSummaryDto?> Handle(GetCatalogItemByCodeQuery request, CancellationToken cancellationToken)
        => service.GetByCodeAsync(request.Kind, request.Code, cancellationToken);
}

public sealed class CatalogItemExistsByIdQueryHandler(CatalogItemService service) : IRequestHandler<CatalogItemExistsByIdQuery, CatalogExistsDto>
{
    public Task<CatalogExistsDto> Handle(CatalogItemExistsByIdQuery request, CancellationToken cancellationToken)
        => service.ExistsByIdAsync(request.Kind, request.Id, cancellationToken);
}

public sealed class CatalogItemExistsByCodeQueryHandler(CatalogItemService service) : IRequestHandler<CatalogItemExistsByCodeQuery, CatalogExistsDto>
{
    public Task<CatalogExistsDto> Handle(CatalogItemExistsByCodeQuery request, CancellationToken cancellationToken)
        => service.ExistsByCodeAsync(request.Kind, request.Code, cancellationToken);
}

public sealed class CreateCatalogItemCommandHandler(CatalogItemService service) : IRequestHandler<CreateCatalogItemCommand, ProductCatalogSummaryDto>
{
    public Task<ProductCatalogSummaryDto> Handle(CreateCatalogItemCommand request, CancellationToken cancellationToken)
        => service.CreateAsync(
            request.Kind,
            request.Code,
            request.Name,
            request.Description,
            request.CategoryId,
            request.UnitPrice,
            request.CurrencyCode,
            request.DefaultDiscountRate,
            request.DefaultTaxRate,
            cancellationToken);
}

public sealed class UpdateCatalogItemCommandHandler(CatalogItemService service) : IRequestHandler<UpdateCatalogItemCommand, ProductCatalogSummaryDto>
{
    public Task<ProductCatalogSummaryDto> Handle(UpdateCatalogItemCommand request, CancellationToken cancellationToken)
        => service.UpdateAsync(
            request.Kind,
            request.Id,
            request.Code,
            request.Name,
            request.Description,
            request.CategoryId,
            request.UnitPrice,
            request.CurrencyCode,
            request.DefaultDiscountRate,
            request.DefaultTaxRate,
            cancellationToken);
}

public sealed class PatchCatalogItemCommandHandler(CatalogItemService service) : IRequestHandler<PatchCatalogItemCommand, ProductCatalogSummaryDto>
{
    public Task<ProductCatalogSummaryDto> Handle(PatchCatalogItemCommand request, CancellationToken cancellationToken)
        => service.PatchAsync(
            request.Kind,
            request.Id,
            request.Code,
            request.Name,
            request.Description,
            request.IsActive,
            request.CategoryId,
            request.UnitPrice,
            request.CurrencyCode,
            request.DefaultDiscountRate,
            request.DefaultTaxRate,
            cancellationToken);
}

public sealed class DeleteCatalogItemCommandHandler(CatalogItemService service) : IRequestHandler<DeleteCatalogItemCommand>
{
    public Task Handle(DeleteCatalogItemCommand request, CancellationToken cancellationToken)
        => service.DeleteAsync(request.Kind, request.Id, cancellationToken);
}

public sealed class SetCatalogItemActiveStateCommandHandler(CatalogItemService service) : IRequestHandler<SetCatalogItemActiveStateCommand, ProductCatalogSummaryDto>
{
    public Task<ProductCatalogSummaryDto> Handle(SetCatalogItemActiveStateCommand request, CancellationToken cancellationToken)
        => service.SetActiveStateAsync(request.Kind, request.Id, request.IsActive, cancellationToken);
}

public sealed class BulkCreateCatalogItemsCommandHandler(CatalogItemService service) : IRequestHandler<BulkCreateCatalogItemsCommand, CatalogBulkOperationResultDto>
{
    public Task<CatalogBulkOperationResultDto> Handle(BulkCreateCatalogItemsCommand request, CancellationToken cancellationToken)
        => service.BulkCreateAsync(request.Kind, request.Items, cancellationToken);
}

public sealed class BulkUpdateCatalogItemsCommandHandler(CatalogItemService service) : IRequestHandler<BulkUpdateCatalogItemsCommand, CatalogBulkOperationResultDto>
{
    public Task<CatalogBulkOperationResultDto> Handle(BulkUpdateCatalogItemsCommand request, CancellationToken cancellationToken)
        => service.BulkUpdateAsync(request.Kind, request.Items, cancellationToken);
}

public sealed class BulkDeleteCatalogItemsCommandHandler(CatalogItemService service) : IRequestHandler<BulkDeleteCatalogItemsCommand, CatalogBulkOperationResultDto>
{
    public Task<CatalogBulkOperationResultDto> Handle(BulkDeleteCatalogItemsCommand request, CancellationToken cancellationToken)
        => service.BulkDeleteAsync(request.Kind, request.Ids, cancellationToken);
}

public sealed class BulkSetCatalogItemsActiveStateCommandHandler(CatalogItemService service) : IRequestHandler<BulkSetCatalogItemsActiveStateCommand, CatalogBulkOperationResultDto>
{
    public Task<CatalogBulkOperationResultDto> Handle(BulkSetCatalogItemsActiveStateCommand request, CancellationToken cancellationToken)
        => service.BulkSetActiveStateAsync(request.Kind, request.Ids, request.IsActive, cancellationToken);
}

public sealed class ImportCatalogItemsCommandHandler(CatalogItemService service) : IRequestHandler<ImportCatalogItemsCommand, CatalogImportResultDto>
{
    public Task<CatalogImportResultDto> Handle(ImportCatalogItemsCommand request, CancellationToken cancellationToken)
        => service.ImportAsync(request.Kind, request.CsvContent, request.UpsertExisting, cancellationToken);
}

public sealed class ExportCatalogItemsQueryHandler(CatalogItemService service) : IRequestHandler<ExportCatalogItemsQuery, ExportFileDto>
{
    public Task<ExportFileDto> Handle(ExportCatalogItemsQuery request, CancellationToken cancellationToken)
        => service.ExportAsync(request.Kind, request.Search, request.Code, request.Name, request.IsActive, cancellationToken);
}

public sealed class ExportCatalogItemsTemplateQueryHandler : IRequestHandler<ExportCatalogItemsTemplateQuery, ExportFileDto>
{
    public Task<ExportFileDto> Handle(ExportCatalogItemsTemplateQuery request, CancellationToken cancellationToken)
        => Task.FromResult(CatalogItemService.ExportTemplate(request.Kind));
}

public sealed class GetProductCatalogMetaQueryHandler : IRequestHandler<GetProductCatalogMetaQuery, ProductCatalogMetaDto>
{
    public Task<ProductCatalogMetaDto> Handle(GetProductCatalogMetaQuery request, CancellationToken cancellationToken)
        => Task.FromResult(CatalogItemService.GetMeta());
}

public sealed class GetProductCatalogStatsQueryHandler(CatalogItemService service) : IRequestHandler<GetProductCatalogStatsQuery, ProductCatalogStatsDto>
{
    public Task<ProductCatalogStatsDto> Handle(GetProductCatalogStatsQuery request, CancellationToken cancellationToken)
        => service.GetStatsAsync(cancellationToken);
}

public sealed class GetProductCatalogLookupsQueryHandler(CatalogItemService service) : IRequestHandler<GetProductCatalogLookupsQuery, ProductCatalogLookupsDto>
{
    public Task<ProductCatalogLookupsDto> Handle(GetProductCatalogLookupsQuery request, CancellationToken cancellationToken)
        => service.GetLookupsAsync(cancellationToken);
}
