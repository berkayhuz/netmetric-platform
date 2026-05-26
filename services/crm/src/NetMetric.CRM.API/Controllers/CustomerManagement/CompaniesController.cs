// <copyright file="CompaniesController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.CustomerManagement.Application.Commands.Companies;
using NetMetric.CRM.CustomerManagement.Application.Queries.Companies;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;
using NetMetric.CRM.CustomerManagement.Contracts.Requests;
using NetMetric.CRM.Types;
using NetMetric.Pagination;

namespace NetMetric.CRM.API.Controllers.CustomerManagement;

[ApiController]
[Route("api/companies")]
public sealed class CompaniesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.CompaniesRead)]
    public async Task<ActionResult<PagedResult<CompanyListItemDto>>> Get(
        [FromQuery] string? search,
        [FromQuery] CompanyType? companyType,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new GetCompaniesQuery(search, companyType, isActive, page, pageSize), cancellationToken));

    [HttpGet("{companyId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CompaniesRead)]
    public async Task<ActionResult<CompanyDetailDto>> GetById(Guid companyId, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetCompanyByIdQuery(companyId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.CompaniesManage)]
    public async Task<ActionResult<CompanyDetailDto>> Create([FromBody] CompanyUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new CreateCompanyCommand(
            request.Name,
            request.TaxNumber,
            request.TaxOffice,
            request.Website,
            request.Email,
            request.Phone,
            request.Sector,
            request.EmployeeCountRange,
            request.AnnualRevenue,
            request.Description,
            request.Notes,
            request.CompanyType,
            request.OwnerUserId,
            request.ParentCompanyId), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { companyId = result.Id }, result);
    }

    [HttpPut("{companyId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CompaniesManage)]
    public async Task<ActionResult<CompanyDetailDto>> Update(Guid companyId, [FromBody] CompanyUpsertRequest request, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new UpdateCompanyCommand(
            companyId,
            request.Name,
            request.TaxNumber,
            request.TaxOffice,
            request.Website,
            request.Email,
            request.Phone,
            request.Sector,
            request.EmployeeCountRange,
            request.AnnualRevenue,
            request.Description,
            request.Notes,
            request.CompanyType,
            request.OwnerUserId,
            request.ParentCompanyId,
            request.RowVersion), cancellationToken));

    [HttpPost("{companyId:guid}/logo")]
    [Authorize(Policy = AuthorizationPolicies.CompaniesManage)]
    public async Task<ActionResult<CompanyDetailDto>> UploadLogo(
        Guid companyId,
        [FromForm] IFormFile? file,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("Logo file is required.");
        }

        await using var stream = file.OpenReadStream();
        return Ok(await mediator.Send(new UploadCompanyLogoCommand(
            companyId,
            file.FileName,
            file.ContentType,
            stream,
            file.Length), cancellationToken));
    }

    [HttpDelete("{companyId:guid}/logo")]
    [Authorize(Policy = AuthorizationPolicies.CompaniesManage)]
    public async Task<ActionResult<CompanyDetailDto>> RemoveLogo(Guid companyId, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new RemoveCompanyLogoCommand(companyId), cancellationToken));

    [HttpPost("{companyId:guid}/activate")]
    [Authorize(Policy = AuthorizationPolicies.CompaniesManage)]
    public async Task<IActionResult> Activate(Guid companyId, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new ActivateCompanyCommand(companyId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{companyId:guid}/deactivate")]
    [Authorize(Policy = AuthorizationPolicies.CompaniesManage)]
    public async Task<IActionResult> Deactivate(Guid companyId, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new DeactivateCompanyCommand(companyId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{companyId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CompaniesManage)]
    public async Task<IActionResult> SoftDelete(Guid companyId, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new SoftDeleteCompanyCommand(companyId), cancellationToken);
        return NoContent();
    }
}
