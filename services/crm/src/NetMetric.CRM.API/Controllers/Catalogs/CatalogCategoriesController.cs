// <copyright file="CatalogCategoriesController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.ProductCatalog.Application.Common;
using NetMetric.CRM.ProductCatalog.Application.Features.CatalogItems;
using NetMetric.CRM.ProductCatalog.Contracts.DTOs;
using NetMetric.CRM.ProductCatalog.Contracts.Requests;
using NetMetric.Pagination;

namespace NetMetric.CRM.API.Controllers.Catalogs;

[ApiController]
[Route("api/catalog/categories")]
[Authorize(Policy = AuthorizationPolicies.CatalogProductsRead)]
public sealed class CatalogCategoriesController(IMediator mediator) : ControllerBase
{
    private const CatalogEntityKind CategoryKind = CatalogEntityKind.Categories;

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductCatalogSummaryDto>>> Get(
        [FromQuery] string? search,
        [FromQuery] string? code,
        [FromQuery] string? name,
        [FromQuery] bool? isActive,
        [FromQuery] bool includeDeleted = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "name",
        [FromQuery] string? sortDirection = "asc",
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetCatalogItemsQuery(CategoryKind, search, code, name, isActive, includeDeleted, page, pageSize, sortBy, sortDirection),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{categoryId:guid}")]
    public async Task<ActionResult<ProductCatalogSummaryDto>> GetById(
        Guid categoryId,
        [FromQuery] bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetCatalogItemByIdQuery(CategoryKind, categoryId, includeDeleted), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.CatalogProductsManage)]
    public async Task<ActionResult<ProductCatalogSummaryDto>> Create(
        [FromBody] CatalogItemUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateCategoryPayload(request.Code, request.Name);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var result = await mediator.Send(
            new CreateCatalogItemCommand(CategoryKind, request.Code, request.Name, request.Description),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { categoryId = result.Id }, result);
    }

    [HttpPut("{categoryId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CatalogProductsManage)]
    public async Task<ActionResult<ProductCatalogSummaryDto>> Update(
        Guid categoryId,
        [FromBody] CatalogItemUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateCategoryPayload(request.Code, request.Name);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var result = await mediator.Send(
            new UpdateCatalogItemCommand(CategoryKind, categoryId, request.Code, request.Name, request.Description),
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{categoryId:guid}/active-state")]
    [Authorize(Policy = AuthorizationPolicies.CatalogProductsManage)]
    public async Task<ActionResult<ProductCatalogSummaryDto>> SetActiveState(
        Guid categoryId,
        [FromBody] SetCategoryActiveStateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new SetCatalogItemActiveStateCommand(CategoryKind, categoryId, request.IsActive),
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{categoryId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CatalogProductsManage)]
    public async Task<IActionResult> Delete(Guid categoryId, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteCatalogItemCommand(CategoryKind, categoryId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{categoryId:guid}/image")]
    [Authorize(Policy = AuthorizationPolicies.CatalogProductsManage)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<object>> UploadImage(Guid categoryId, IFormFile? file, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0) return BadRequest("Image file is required.");
        await using var stream = file.OpenReadStream();
        var url = await mediator.Send(new UploadCategoryImageCommand(categoryId, file.FileName, file.ContentType ?? "application/octet-stream", stream, file.Length), cancellationToken);
        return Ok(new { publicUrl = url });
    }

    [HttpDelete("{categoryId:guid}/image")]
    [Authorize(Policy = AuthorizationPolicies.CatalogProductsManage)]
    public async Task<IActionResult> RemoveImage(Guid categoryId, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new RemoveCategoryImageCommand(categoryId), cancellationToken);
        return NoContent();
    }

    public sealed record SetCategoryActiveStateRequest([property: JsonRequired] bool IsActive);

    private static string? ValidateCategoryPayload(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return "Code is required.";
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return "Name is required.";
        }

        return null;
    }
}
