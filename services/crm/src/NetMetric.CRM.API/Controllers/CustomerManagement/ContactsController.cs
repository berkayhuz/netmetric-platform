// <copyright file="ContactsController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.CustomerManagement.Application.Commands.Contacts;
using NetMetric.CRM.CustomerManagement.Application.Queries.Contacts;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;
using NetMetric.CRM.CustomerManagement.Contracts.Requests;
using NetMetric.Pagination;

namespace NetMetric.CRM.API.Controllers.CustomerManagement;

[ApiController]
[Route("api/contacts")]
[Route("api/v1/contacts")]
public sealed class ContactsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.ContactsRead)]
    public async Task<ActionResult<PagedResult<ContactListItemDto>>> Get(
        [FromQuery] string? search,
        [FromQuery] Guid? companyId,
        [FromQuery] Guid? customerId,
        [FromQuery] bool? isPrimary,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null,
        CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new GetContactsQuery(search, companyId, customerId, isPrimary, isActive, page, pageSize, sortBy, sortDirection), cancellationToken));

    [HttpGet("{contactId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ContactsRead)]
    public async Task<ActionResult<ContactDetailDto>> GetById(Guid contactId, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetContactByIdQuery(contactId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ContactsManage)]
    public async Task<ActionResult<ContactDetailDto>> Create([FromBody] ContactUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new CreateContactCommand(
            request.FirstName,
            request.LastName,
            request.Title,
            request.Email,
            request.MobilePhone,
            request.WorkPhone,
            request.PersonalPhone,
            request.BirthDate,
            request.Gender,
            request.Department,
            request.JobTitle,
            request.Description,
            request.Notes,
            request.OwnerUserId,
            request.CompanyId,
            request.CustomerId,
            request.IsPrimaryContact), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { contactId = result.Id }, result);
    }

    [HttpPut("{contactId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ContactsManage)]
    public async Task<ActionResult<ContactDetailDto>> Update(Guid contactId, [FromBody] ContactUpsertRequest request, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new UpdateContactCommand(
            contactId,
            request.FirstName,
            request.LastName,
            request.Title,
            request.Email,
            request.MobilePhone,
            request.WorkPhone,
            request.PersonalPhone,
            request.BirthDate,
            request.Gender,
            request.Department,
            request.JobTitle,
            request.Description,
            request.Notes,
            request.OwnerUserId,
            request.CompanyId,
            request.CustomerId,
            request.IsPrimaryContact,
            request.RowVersion), cancellationToken));

    [HttpPost("{contactId:guid}/set-primary")]
    [Authorize(Policy = AuthorizationPolicies.ContactsManage)]
    public async Task<IActionResult> SetPrimary(Guid contactId, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new SetPrimaryContactCommand(contactId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{contactId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ContactsManage)]
    public async Task<IActionResult> SoftDelete(Guid contactId, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new SoftDeleteContactCommand(contactId), cancellationToken);
        return NoContent();
    }
}
