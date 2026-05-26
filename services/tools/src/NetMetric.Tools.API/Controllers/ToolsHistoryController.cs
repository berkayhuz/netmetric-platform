// <copyright file="ToolsHistoryController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.Tools.API.DependencyInjection;
using NetMetric.Tools.Application.History.Commands;
using NetMetric.Tools.Application.History.Queries;
using NetMetric.Tools.Contracts.History;

namespace NetMetric.Tools.API.Controllers;

[ApiController]
[Route("api/v1/tools/history")]
public sealed class ToolsHistoryController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = ToolsPolicies.ToolsHistoryReadOwn)]
    [ProducesResponseType<ToolHistoryPageResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ToolHistoryPageResponse>> GetHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? toolSlug = null, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new GetMyToolRunsQuery(new ToolHistoryQuery(page, pageSize, toolSlug)), cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = ToolsPolicies.ToolsHistoryReadOwn)]
    [ProducesResponseType<ToolRunDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ToolRunDetailResponse>> GetHistoryDetail([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetMyToolRunDetailQuery(id), cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost]
    [Authorize(Policy = ToolsPolicies.ToolsHistoryWriteOwn)]
    [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [ProducesResponseType<CreateToolRunResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateToolRunResponse>> CreateHistory([FromForm] CreateToolRunFormRequest request, CancellationToken cancellationToken)
    {
        if (request.Artifact is null || request.Artifact.Length == 0)
        {
            return BadRequest("Artifact file is required.");
        }

        await using var artifactStream = request.Artifact.OpenReadStream();

        var command = new CreateMyToolRunCommand(
            new CreateToolRunRequest(request.ToolSlug, request.InputSummaryJson ?? "{}", request.Artifact.FileName, request.Artifact.ContentType),
            artifactStream,
            request.Artifact.Length);

        var response = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetHistoryDetail), new { id = response.RunId }, response);
    }

    [HttpGet("{id:guid}/download")]
    [Authorize(Policy = ToolsPolicies.ToolsArtifactDownloadOwn)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetMyToolArtifactDownloadQuery(id), cancellationToken);
        if (response is null)
        {
            return NotFound();
        }

        Response.Headers["X-Content-Type-Options"] = "nosniff";
        Response.Headers["Cache-Control"] = "private, max-age=300";
        return File(response.Value.Content, response.Value.MimeType, response.Value.FileName);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = ToolsPolicies.ToolsHistoryDeleteOwn)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var deleted = await mediator.Send(new DeleteMyToolRunCommand(id), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}

public sealed class CreateToolRunFormRequest
{
    public string ToolSlug { get; init; } = string.Empty;
    public string? InputSummaryJson { get; init; }
    public IFormFile? Artifact { get; init; }
}
