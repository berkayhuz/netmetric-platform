// <copyright file="ToolsJobsController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.Tools.API.DependencyInjection;
using NetMetric.Tools.API.Jobs;
using NetMetric.Tools.Contracts.History;
using NetMetric.Tools.Contracts.Jobs;

namespace NetMetric.Tools.API.Controllers;

[ApiController]
[Route("api/v1/tools/jobs")]
public sealed class ToolsJobsController(IToolJobQueue jobQueue) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = ToolsPolicies.ToolsHistoryWriteOwn)]
    [ProducesResponseType<CreateToolJobResponse>(StatusCodes.Status202Accepted)]
    public async Task<ActionResult<CreateToolJobResponse>> Create([FromForm] CreateToolRunFormRequest request, CancellationToken cancellationToken)
    {
        if (request.Artifact is null || request.Artifact.Length == 0)
        {
            return BadRequest("Artifact file is required.");
        }

        await using var stream = request.Artifact.OpenReadStream();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken);

        var response = await jobQueue.EnqueueAsync(
            new CreateToolRunRequest(request.ToolSlug, request.InputSummaryJson ?? "{}", request.Artifact.FileName, request.Artifact.ContentType),
            ms.ToArray(),
            request.Artifact.Length,
            cancellationToken);

        return Accepted(response);
    }

    [HttpGet("{jobId:guid}")]
    [Authorize(Policy = ToolsPolicies.ToolsHistoryReadOwn)]
    [ProducesResponseType<ToolJobStatusResponse>(StatusCodes.Status200OK)]
    public ActionResult<ToolJobStatusResponse> Status([FromRoute] Guid jobId)
        => jobQueue.TryGet(jobId, out var response) ? Ok(response) : NotFound();
}
