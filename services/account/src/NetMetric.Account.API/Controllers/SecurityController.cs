// <copyright file="SecurityController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NetMetric.Account.Api.DependencyInjection;
using NetMetric.Account.Api.Http;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Security.Email;
using NetMetric.Account.Application.Security.Mfa;
using NetMetric.Account.Application.Security.Password;
using NetMetric.Account.Application.Security.RecoveryCodes;
using NetMetric.Account.Contracts.Security;

namespace NetMetric.Account.Api.Controllers;

[ApiController]
[Route("api/v1/account/security")]
public sealed class SecurityController(IMediator mediator) : ControllerBase
{
    [HttpPost("password/change")]
    [Authorize(Policy = AccountPolicies.SecurityChangePassword)]
    [EnableRateLimiting(AccountOperationalHardeningExtensions.CriticalRateLimitPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
        => (await mediator.Send(new ChangePasswordCommand(request.CurrentPassword, request.NewPassword, request.ConfirmNewPassword), cancellationToken)).ToActionResult();

    [HttpPost("email/change-request")]
    [Authorize(Policy = AccountPolicies.SecurityChangePassword)]
    [EnableRateLimiting(AccountOperationalHardeningExtensions.CriticalRateLimitPolicy)]
    [ProducesResponseType<EmailChangeRequestResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<EmailChangeRequestResponse>> RequestEmailChange(
        [FromBody] EmailChangeRequest request,
        CancellationToken cancellationToken)
        => (await mediator.Send(new RequestEmailChangeCommand(request.NewEmail, request.CurrentPassword), cancellationToken)).ToActionResult();

    [HttpPost("email/change-confirm")]
    [Authorize(Policy = AccountPolicies.SecurityChangePassword)]
    [EnableRateLimiting(AccountOperationalHardeningExtensions.CriticalRateLimitPolicy)]
    [ProducesResponseType<EmailChangeConfirmResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<EmailChangeConfirmResponse>> ConfirmEmailChange(
        [FromBody] EmailChangeConfirmRequest request,
        CancellationToken cancellationToken)
        => (await mediator.Send(new ConfirmEmailChangeCommand(request.Token), cancellationToken)).ToActionResult();

    [HttpGet("mfa")]
    [Authorize(Policy = AccountPolicies.SecurityReadOwn)]
    [ProducesResponseType<MfaStatusResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<MfaStatusResponse>> GetMfa(CancellationToken cancellationToken)
        => (await mediator.Send(new GetMfaStatusQuery(), cancellationToken)).ToActionResult();

    [HttpPost("mfa/setup")]
    [Authorize(Policy = AccountPolicies.SecurityManageMfa)]
    [EnableRateLimiting(AccountOperationalHardeningExtensions.CriticalRateLimitPolicy)]
    [ProducesResponseType<MfaSetupResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<MfaSetupResponse>> SetupMfa(CancellationToken cancellationToken)
        => (await mediator.Send(new StartMfaSetupCommand(), cancellationToken)).ToActionResult();

    [HttpPost("mfa/confirm")]
    [Authorize(Policy = AccountPolicies.SecurityManageMfa)]
    [EnableRateLimiting(AccountOperationalHardeningExtensions.CriticalRateLimitPolicy)]
    [ProducesResponseType<ConfirmMfaResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ConfirmMfaResponse>> ConfirmMfa([FromBody] ConfirmMfaRequest request, CancellationToken cancellationToken)
        => (await mediator.Send(new ConfirmMfaCommand(request.VerificationCode), cancellationToken)).ToActionResult();

    [HttpDelete("mfa")]
    [Authorize(Policy = AccountPolicies.SecurityManageMfa)]
    [EnableRateLimiting(AccountOperationalHardeningExtensions.CriticalRateLimitPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DisableMfa([FromBody] DisableMfaRequest request, CancellationToken cancellationToken)
        => (await mediator.Send(new DisableMfaCommand(request.VerificationCode), cancellationToken)).ToActionResult();

    [HttpPost("mfa/recovery-codes/regenerate")]
    [Authorize(Policy = AccountPolicies.SecurityManageMfa)]
    [EnableRateLimiting(AccountOperationalHardeningExtensions.CriticalRateLimitPolicy)]
    [ProducesResponseType<RecoveryCodesResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<RecoveryCodesResponse>> RegenerateRecoveryCodes(CancellationToken cancellationToken)
        => (await mediator.Send(new RegenerateRecoveryCodesCommand(), cancellationToken)).ToActionResult();
}
