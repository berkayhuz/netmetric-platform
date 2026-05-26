// <copyright file="AuditController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NetMetric.Account.Api.DependencyInjection;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Audit.Queries;
using NetMetric.Account.Contracts.Audit;

namespace NetMetric.Account.Api.Controllers;

[ApiController]
[Route("api/v1/account/audit")]
public sealed class AuditController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AccountPolicies.AccountRead)]
    [EnableRateLimiting(AccountOperationalHardeningExtensions.CriticalRateLimitPolicy)]
    public async Task<ActionResult<AccountAuditEntriesResponse>> List(
        [FromQuery] int limit = 50,
        [FromQuery] string? eventType = null,
        CancellationToken cancellationToken = default)
    {
        var entries = await mediator.Send(new GetAccountAuditEntriesQuery(limit, eventType), cancellationToken);
        return Ok(new AccountAuditEntriesResponse(
            entries.Select(x => new AccountAuditEntryResponse(
                x.Id,
                x.TenantId,
                x.UserId,
                x.EventType,
                x.Severity,
                x.OccurredAt,
                x.CorrelationId,
                x.MetadataJson)).ToArray(),
            entries.Count));
    }
}
