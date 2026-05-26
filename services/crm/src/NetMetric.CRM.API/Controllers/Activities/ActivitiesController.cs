// <copyright file="ActivitiesController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.API.Contracts.Activities;
using NetMetric.CRM.API.Features.Activities;

namespace NetMetric.CRM.API.Controllers.Activities;

[ApiController]
[Route("api/activities")]
[Authorize(Policy = AuthorizationPolicies.ActivitiesRead)]
public sealed class ActivitiesController(
    IActivityTimelineReadService timelineReadService,
    IActivityTimelineWriteService timelineWriteService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ActivityTimelineFeedDto>> Get(
        [FromQuery] string? type,
        [FromQuery] string? sourceModule,
        [FromQuery] Guid? ownerUserId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Ok(await timelineReadService.GetGlobalAsync(type, sourceModule, ownerUserId, fromUtc, toUtc, page, pageSize, cancellationToken));

    [HttpGet("{activityId:guid}")]
    public async Task<ActionResult<ActivityTimelineItemDto>> GetById(Guid activityId, CancellationToken cancellationToken = default)
    {
        var item = await timelineReadService.GetByIdAsync(activityId, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("related/{entityType}/{entityId:guid}")]
    public async Task<ActionResult<ActivityTimelineFeedDto>> GetRelated(
        string entityType,
        Guid entityId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!ActivityEntityTypeParser.TryParse(entityType, out var parsedEntityType))
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["entityType"] =
                [
                    "Unsupported entity type. Supported values: customer, company, contact, lead, deal, opportunity, quote, ticket, task."
                ]
            }));
        }

        var feed = await timelineReadService.GetRelatedAsync(parsedEntityType, entityId, fromUtc, toUtc, page, pageSize, cancellationToken);
        return Ok(feed);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ActivitiesCreate)]
    public async Task<ActionResult<CreateActivityResponseDto>> Create(
        [FromBody] CreateActivityRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["request"] = ["Request body is required."]
            }));
        }

        try
        {
            var created = await timelineWriteService.CreateAsync(request, cancellationToken);
            return Ok(created);
        }
        catch (ActivityValidationException validationException)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [validationException.Key] = [validationException.Message]
            }));
        }
        catch (KeyNotFoundException keyNotFoundException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Related record not found.",
                Detail = keyNotFoundException.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (UnauthorizedAccessException unauthorizedAccessException)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Title = "Forbidden.",
                Detail = unauthorizedAccessException.Message,
                Status = StatusCodes.Status403Forbidden
            });
        }
    }
}
