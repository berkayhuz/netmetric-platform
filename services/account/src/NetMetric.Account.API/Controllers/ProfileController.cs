// <copyright file="ProfileController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetMetric.Account.Api.Http;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Profiles.Commands;
using NetMetric.Account.Application.Profiles.Queries;
using NetMetric.Account.Contracts.Profiles;

namespace NetMetric.Account.Api.Controllers;

[ApiController]
[Route("api/v1/account/profile")]
public sealed class ProfileController(IMediator mediator) : ControllerBase
{
    private const long AvatarUploadLimitBytes = 10 * 1024 * 1024;

    [HttpGet]
    [Authorize(Policy = AccountPolicies.ProfileReadOwn)]
    [ProducesResponseType<MyProfileResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<MyProfileResponse>> Get(CancellationToken cancellationToken)
        => (await mediator.Send(new GetMyProfileQuery(), cancellationToken)).ToActionResult();

    [HttpPut]
    [Authorize(Policy = AccountPolicies.ProfileUpdateOwn)]
    [ProducesResponseType<MyProfileResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<MyProfileResponse>> Update(
        [FromBody] UpdateMyProfileRequest request,
        CancellationToken cancellationToken)
        => (await mediator.Send(new UpdateMyProfileCommand(request), cancellationToken)).ToActionResult();

    [HttpPost("avatar")]
    [Authorize(Policy = AccountPolicies.ProfileUpdateOwn)]
    [RequestSizeLimit(AvatarUploadLimitBytes)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<AvatarUploadResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AvatarUploadResponse>> UploadAvatar(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("Avatar file is required.");
        }

        await using var stream = file.OpenReadStream();
        var result = await mediator.Send(
            new UploadMyAvatarCommand(file.FileName, file.ContentType ?? "application/octet-stream", stream, file.Length),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpDelete("avatar")]
    [Authorize(Policy = AccountPolicies.ProfileUpdateOwn)]
    public async Task<IActionResult> RemoveAvatar(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RemoveMyAvatarCommand(), cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound();
    }
}
