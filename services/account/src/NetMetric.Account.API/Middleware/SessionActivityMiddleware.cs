// <copyright file="SessionActivityMiddleware.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Security.Claims;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Domain.Common;
using NetMetric.Account.Domain.Sessions;
using NetMetric.Clock;

namespace NetMetric.Account.Api.Middleware;

public sealed class SessionActivityMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IRepository<IAccountDbContext, UserSession> sessions,
        IAccountDbContext dbContext,
        IClock clock)
    {
        await TrackSessionAsync(context, sessions, dbContext, clock);
        await next(context);
    }

    private static async Task TrackSessionAsync(
        HttpContext context,
        IRepository<IAccountDbContext, UserSession> sessions,
        IAccountDbContext dbContext,
        IClock clock)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var tenantId = ReadGuid(context.User, "tenant_id") ?? ReadGuid(context.User, "tenantId");
        var userId = ReadGuid(context.User, ClaimTypes.NameIdentifier) ?? ReadGuid(context.User, "sub") ?? ReadGuid(context.User, "user_id");
        var sessionId = ReadGuid(context.User, "sid") ?? ReadGuid(context.User, "session_id");
        if (tenantId is null || userId is null || sessionId is null)
        {
            return;
        }

        var utcNow = clock.UtcNow;
        var expiresAt = ReadUnixTime(context.User, "exp") ?? utcNow.AddDays(30);
        var tenant = TenantId.From(tenantId.Value);
        var user = UserId.From(userId.Value);

        var record = await sessions.Query.FirstOrDefaultAsync(
            x => x.TenantId == tenant && x.UserId == user && x.Id == sessionId.Value,
            context.RequestAborted);

        if (record is null)
        {
            record = UserSession.Create(
                sessionId.Value,
                tenant,
                user,
                utcNow,
                expiresAt,
                context.Connection.RemoteIpAddress?.ToString(),
                context.Request.Headers.UserAgent.ToString(),
                context.Request.Headers["X-Device-Name"].ToString());
            await sessions.AddAsync(record, context.RequestAborted);
        }
        else
        {
            record.Touch(
                utcNow,
                expiresAt,
                context.Connection.RemoteIpAddress?.ToString(),
                context.Request.Headers.UserAgent.ToString());
        }

        try
        {
            await dbContext.SaveChangesAsync(context.RequestAborted);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Session activity is best-effort; concurrent updates must not fail the request pipeline.
        }
        catch (DbUpdateException ex) when (IsDuplicateSessionInsert(ex))
        {
            // Concurrent requests may race to create the same session row.
            foreach (var entry in ex.Entries)
            {
                entry.State = EntityState.Detached;
            }

            var existing = await sessions.Query.FirstOrDefaultAsync(
                x => x.TenantId == tenant && x.UserId == user && x.Id == sessionId.Value,
                context.RequestAborted);

            if (existing is null)
            {
                throw;
            }

            existing.Touch(
                utcNow,
                expiresAt,
                context.Connection.RemoteIpAddress?.ToString(),
                context.Request.Headers.UserAgent.ToString());

            try
            {
                await dbContext.SaveChangesAsync(context.RequestAborted);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Session activity is best-effort; concurrent updates must not fail the request pipeline.
            }
        }
    }

    private static Guid? ReadGuid(ClaimsPrincipal principal, string claimType)
        => Guid.TryParse(principal.Claims.FirstOrDefault(claim => claim.Type == claimType)?.Value, out var value) ? value : null;

    private static DateTimeOffset? ReadUnixTime(ClaimsPrincipal principal, string claimType)
        => long.TryParse(principal.Claims.FirstOrDefault(claim => claim.Type == claimType)?.Value, out var value)
            ? DateTimeOffset.FromUnixTimeSeconds(value)
            : null;

    private static bool IsDuplicateSessionInsert(DbUpdateException ex) =>
        ex.InnerException is SqlException sqlEx &&
        (sqlEx.Number == 2627 || sqlEx.Number == 2601);
}
