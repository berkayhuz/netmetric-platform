// <copyright file="CatalogProductsController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.ProductCatalog.Application.Common;
using NetMetric.CRM.ProductCatalog.Application.Features.CatalogItems;
using NetMetric.CRM.ProductCatalog.Contracts.DTOs;
using NetMetric.CRM.ProductCatalog.Contracts.Requests;
using NetMetric.Pagination;

namespace NetMetric.CRM.API.Controllers.Catalogs;

[ApiController]
[Route("api/catalog/products")]
[Authorize(Policy = AuthorizationPolicies.CatalogProductsRead)]
public sealed class CatalogProductsController(IMediator mediator) : ControllerBase
{
    private const CatalogEntityKind ProductKind = CatalogEntityKind.Products;

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductCatalogSummaryDto>>> Get(
        [FromQuery] string? search,
        [FromQuery] string? code,
        [FromQuery] string? name,
        [FromQuery] Guid? categoryId,
        [FromQuery] bool? isActive,
        [FromQuery] bool includeDeleted = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "code",
        [FromQuery] string? sortDirection = "asc",
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetCatalogItemsQuery(ProductKind, search, code, name, isActive, includeDeleted, page, pageSize, sortBy, sortDirection, categoryId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{productId:guid}")]
    public async Task<ActionResult<ProductCatalogSummaryDto>> GetById(
        Guid productId,
        [FromQuery] bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetCatalogItemByIdQuery(ProductKind, productId, includeDeleted), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("by-code/{code}")]
    public async Task<ActionResult<ProductCatalogSummaryDto>> GetByCode(string code, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCatalogItemByCodeQuery(ProductKind, code), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{productId:guid}/exists")]
    public async Task<ActionResult<CatalogExistsDto>> ExistsById(Guid productId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CatalogItemExistsByIdQuery(ProductKind, productId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("by-code/{code}/exists")]
    public async Task<ActionResult<CatalogExistsDto>> ExistsByCode(string code, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CatalogItemExistsByCodeQuery(ProductKind, code), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.CatalogProductsManage)]
    public async Task<ActionResult<ProductCatalogSummaryDto>> Create(
        [FromBody] CatalogItemUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateProductPayload(
            request.Code,
            request.Name,
            request.CurrencyCode,
            request.UnitPrice,
            request.DefaultDiscountRate,
            request.DefaultTaxRate);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var result = await mediator.Send(
            new CreateCatalogItemCommand(
                ProductKind,
                request.Code,
                request.Name,
                request.Description,
                request.CategoryId,
                request.UnitPrice,
                request.CurrencyCode,
                request.DefaultDiscountRate,
                request.DefaultTaxRate),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { productId = result.Id }, result);
    }

    [HttpPut("{productId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CatalogProductsManage)]
    public async Task<ActionResult<ProductCatalogSummaryDto>> Update(
        Guid productId,
        [FromBody] CatalogItemUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateProductPayload(
            request.Code,
            request.Name,
            request.CurrencyCode,
            request.UnitPrice,
            request.DefaultDiscountRate,
            request.DefaultTaxRate);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var result = await mediator.Send(
            new UpdateCatalogItemCommand(
                ProductKind,
                productId,
                request.Code,
                request.Name,
                request.Description,
                request.CategoryId,
                request.UnitPrice,
                request.CurrencyCode,
                request.DefaultDiscountRate,
                request.DefaultTaxRate),
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{productId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CatalogProductsManage)]
    public async Task<ActionResult<ProductCatalogSummaryDto>> Patch(
        Guid productId,
        [FromBody] CatalogItemPatchRequest request,
        CancellationToken cancellationToken)
    {
        if (request.UnitPrice.HasValue && request.UnitPrice.Value < 0)
        {
            return BadRequest("Unit price cannot be negative.");
        }
        if (request.DefaultDiscountRate.HasValue && (request.DefaultDiscountRate < 0 || request.DefaultDiscountRate > 100))
        {
            return BadRequest("Default discount rate must be between 0 and 100.");
        }
        if (request.DefaultTaxRate.HasValue && (request.DefaultTaxRate < 0 || request.DefaultTaxRate > 100))
        {
            return BadRequest("Default tax rate must be between 0 and 100.");
        }

        var result = await mediator.Send(
            new PatchCatalogItemCommand(
                ProductKind,
                productId,
                request.Code,
                request.Name,
                request.Description,
                request.IsActive,
                request.CategoryId,
                request.UnitPrice,
                request.CurrencyCode,
                request.DefaultDiscountRate,
                request.DefaultTaxRate),
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{productId:guid}/active-state")]
    [Authorize(Policy = AuthorizationPolicies.CatalogProductsManage)]
    public async Task<ActionResult<ProductCatalogSummaryDto>> SetActiveState(
        Guid productId,
        [FromBody] SetActiveStateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SetCatalogItemActiveStateCommand(ProductKind, productId, request.IsActive), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{productId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CatalogProductsManage)]
    public async Task<IActionResult> Delete(Guid productId, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteCatalogItemCommand(ProductKind, productId), cancellationToken);
        return NoContent();
    }

    [HttpPost("bulk")]
    [Authorize(Policy = AuthorizationPolicies.CatalogProductsManage)]
    public async Task<ActionResult<CatalogBulkOperationResultDto>> BulkCreate(
        [FromBody] BulkCatalogItemCreateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new BulkCreateCatalogItemsCommand(
                ProductKind,
                request.Items.Select(item => (item.Code, item.Name, item.Description)).ToArray()),
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("bulk")]
    [Authorize(Policy = AuthorizationPolicies.CatalogProductsManage)]
    public async Task<ActionResult<CatalogBulkOperationResultDto>> BulkUpdate(
        [FromBody] BulkCatalogItemUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new BulkUpdateCatalogItemsCommand(
                ProductKind,
                request.Items.Select(item => (item.Id, item.Code, item.Name, item.Description)).ToArray()),
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("bulk")]
    [Authorize(Policy = AuthorizationPolicies.CatalogProductsManage)]
    public async Task<ActionResult<CatalogBulkOperationResultDto>> BulkDelete(
        [FromBody] BulkCatalogItemIdsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new BulkDeleteCatalogItemsCommand(ProductKind, request.Ids), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("bulk/active-state")]
    [Authorize(Policy = AuthorizationPolicies.CatalogProductsManage)]
    public async Task<ActionResult<CatalogBulkOperationResultDto>> BulkSetActiveState(
        [FromBody] BulkSetActiveStateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new BulkSetCatalogItemsActiveStateCommand(ProductKind, request.Ids, request.IsActive),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<FileContentResult> Export(
        [FromQuery] string? search,
        [FromQuery] string? code,
        [FromQuery] string? name,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ExportCatalogItemsQuery(ProductKind, search, code, name, isActive), cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
    }

    [HttpGet("template")]
    public async Task<FileContentResult> ExportTemplate(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ExportCatalogItemsTemplateQuery(ProductKind), cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
    }

    [HttpGet("meta")]
    public async Task<ActionResult<ProductCatalogMetaDto>> GetMeta(CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetProductCatalogMetaQuery(), cancellationToken));

    [HttpGet("stats")]
    public async Task<ActionResult<ProductCatalogStatsDto>> GetStats(CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetProductCatalogStatsQuery(), cancellationToken));

    [HttpGet("lookups")]
    public async Task<ActionResult<ProductCatalogLookupsDto>> GetLookups(CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetProductCatalogLookupsQuery(), cancellationToken));

    [HttpPost("{productId:guid}/images")]
    [Authorize(Policy = AuthorizationPolicies.CatalogProductsManage)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ProductImageDto>> UploadImage(
        Guid productId,
        IFormFile? file,
        [FromForm] bool isPrimary = false,
        [FromForm] int sortOrder = 0,
        [FromForm] string? altText = null,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0) return BadRequest("Image file is required.");
        await using var stream = file.OpenReadStream();
        var result = await mediator.Send(new UploadProductImageCommand(productId, file.FileName, file.ContentType ?? "application/octet-stream", stream, file.Length, isPrimary, sortOrder, altText), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{productId:guid}/images")]
    public async Task<ActionResult<IReadOnlyList<ProductImageDto>>> ListImages(Guid productId, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new ListProductImagesQuery(productId), cancellationToken));

    [HttpPatch("{productId:guid}/images/{productImageId:guid}/primary")]
    [Authorize(Policy = AuthorizationPolicies.CatalogProductsManage)]
    public async Task<IActionResult> SetPrimaryImage(Guid productId, Guid productImageId, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new SetPrimaryProductImageCommand(productId, productImageId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{productId:guid}/images/{productImageId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CatalogProductsManage)]
    public async Task<IActionResult> RemoveImage(Guid productId, Guid productImageId, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new RemoveProductImageCommand(productId, productImageId), cancellationToken);
        return NoContent();
    }

    public sealed record SetActiveStateRequest([property: JsonRequired] bool IsActive);

    public sealed record BulkSetActiveStateRequest(IReadOnlyCollection<Guid> Ids, [property: JsonRequired] bool IsActive);

    private static string? ValidateProductPayload(
        string code,
        string name,
        string currencyCode,
        decimal? unitPrice,
        decimal defaultDiscountRate,
        decimal defaultTaxRate)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return "Code is required.";
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return "Name is required.";
        }

        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            return "Currency code is required.";
        }

        if (unitPrice.HasValue && unitPrice.Value < 0)
        {
            return "Unit price cannot be negative.";
        }
        if (defaultDiscountRate < 0 || defaultDiscountRate > 100)
        {
            return "Default discount rate must be between 0 and 100.";
        }
        if (defaultTaxRate < 0 || defaultTaxRate > 100)
        {
            return "Default tax rate must be between 0 and 100.";
        }

        return null;
    }
}
