// <copyright file="TrashController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.CustomerManagement.Application.Commands.Trash;
using NetMetric.CRM.CustomerManagement.Application.Queries.Trash;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;
using NetMetric.Pagination;

namespace NetMetric.CRM.API.Controllers.CustomerManagement;

[ApiController]
[Route("api/trash")]
[Route("api/v1/trash")]
public sealed class TrashController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.ContactsRead)]
    public async Task<ActionResult<PagedResult<GlobalTrashItemListItemDto>>> Get(
        [FromQuery] string? search,
        [FromQuery] string? entityType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null,
        CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(
            new GetGlobalTrashItemsQuery(search, entityType, page, pageSize, sortBy, sortDirection),
            cancellationToken));

    [HttpPost("{trashItemId:guid}/restore")]
    [Authorize]
    public async Task<IActionResult> Restore(Guid trashItemId, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new RestoreTrashItemCommand(trashItemId), cancellationToken);
        return NoContent();
    }
}
