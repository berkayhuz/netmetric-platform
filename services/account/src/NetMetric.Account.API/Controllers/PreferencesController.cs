// <copyright file="PreferencesController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.Account.Api.Http;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Preferences.Commands;
using NetMetric.Account.Application.Preferences.Queries;
using NetMetric.Account.Contracts.Preferences;
using NetMetric.Account.Contracts.Profiles;

namespace NetMetric.Account.Api.Controllers;

[ApiController]
[Route("api/v1/account/preferences")]
public sealed class PreferencesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AccountPolicies.PreferencesReadOwn)]
    [ProducesResponseType<UserPreferenceResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserPreferenceResponse>> Get(CancellationToken cancellationToken)
        => (await mediator.Send(new GetMyPreferencesQuery(), cancellationToken)).ToActionResult();

    [HttpPut]
    [Authorize(Policy = AccountPolicies.PreferencesUpdateOwn)]
    [ProducesResponseType<UserPreferenceResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserPreferenceResponse>> Update(
        [FromBody] UpdateUserPreferenceRequest request,
        CancellationToken cancellationToken)
        => (await mediator.Send(new UpdateMyPreferencesCommand(request), cancellationToken)).ToActionResult();

    [HttpPost("favicon")]
    [Authorize(Policy = AccountPolicies.PreferencesUpdateOwn)]
    [RequestSizeLimit(1_048_576)]
    [ProducesResponseType<AvatarUploadResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AvatarUploadResponse>> UploadFavicon(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        return (await mediator.Send(
            new UploadMyFaviconCommand(file.FileName, file.ContentType, stream, file.Length),
            cancellationToken)).ToActionResult();
    }

    [HttpDelete("favicon")]
    [Authorize(Policy = AccountPolicies.PreferencesUpdateOwn)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteFavicon(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RemoveMyFaviconCommand(), cancellationToken);
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return result.ToActionResult().Result ?? BadRequest();
    }
}
