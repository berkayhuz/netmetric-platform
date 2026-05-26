// <copyright file="NotificationStateCommands.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Common;
using NetMetric.Account.Domain.Audit;
using NetMetric.Account.Domain.Common;
using NetMetric.Account.Domain.Notifications;
using NetMetric.Clock;

namespace NetMetric.Account.Application.Notifications.Commands;

public sealed record MarkNotificationAsReadCommand(Guid NotificationId) : IRequest<Result>;
public sealed record MarkAllNotificationsAsReadCommand : IRequest<Result>;
public sealed record DeleteNotificationCommand(Guid NotificationId) : IRequest<Result>;

public sealed class MarkNotificationAsReadCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IClock clock,
    IRepository<IAccountDbContext, AccountAuditEntry> auditEntries,
    IRepository<IAccountDbContext, UserNotificationState> states,
    IAccountDbContext dbContext)
    : IRequestHandler<MarkNotificationAsReadCommand, Result>
{
    public async Task<Result> Handle(MarkNotificationAsReadCommand command, CancellationToken cancellationToken)
    {
        if (command.NotificationId == Guid.Empty)
        {
            return Result.Failure(Error.Validation("Notification id is required."));
        }

        var current = currentUserAccessor.GetRequired();
        var tenantId = TenantId.From(current.TenantId);
        var userId = UserId.From(current.UserId);

        var exists = await auditEntries.AnyAsync(x => x.Id == command.NotificationId && x.TenantId == tenantId && x.UserId == userId, cancellationToken);
        if (!exists)
        {
            return Result.Failure(Error.NotFound("Notification"));
        }

        var state = await states.Query.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.UserId == userId && x.NotificationId == command.NotificationId,
            cancellationToken);
        var isNew = state is null;
        state ??= UserNotificationState.Create(tenantId, userId, command.NotificationId, clock.UtcNow);
        state.MarkRead(clock.UtcNow);
        if (isNew)
        {
            await states.AddAsync(state, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class MarkAllNotificationsAsReadCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IClock clock,
    IRepository<IAccountDbContext, AccountAuditEntry> auditEntries,
    IRepository<IAccountDbContext, UserNotificationState> states,
    IAccountDbContext dbContext)
    : IRequestHandler<MarkAllNotificationsAsReadCommand, Result>
{
    public async Task<Result> Handle(MarkAllNotificationsAsReadCommand command, CancellationToken cancellationToken)
    {
        var current = currentUserAccessor.GetRequired();
        var tenantId = TenantId.From(current.TenantId);
        var userId = UserId.From(current.UserId);
        var utcNow = clock.UtcNow;

        var ids = await auditEntries.Query
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == userId)
            .OrderByDescending(x => x.OccurredAt)
            .Select(x => x.Id)
            .Take(200)
            .ToListAsync(cancellationToken);

        if (ids.Count == 0)
        {
            return Result.Success();
        }

        var existing = await states.Query
            .Where(x => x.TenantId == tenantId && x.UserId == userId && ids.Contains(x.NotificationId))
            .ToListAsync(cancellationToken);
        var byId = existing.ToDictionary(x => x.NotificationId);

        foreach (var id in ids)
        {
            if (!byId.TryGetValue(id, out var state))
            {
                state = UserNotificationState.Create(tenantId, userId, id, utcNow);
                await states.AddAsync(state, cancellationToken);
            }

            state.MarkRead(utcNow);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeleteNotificationCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IClock clock,
    IRepository<IAccountDbContext, AccountAuditEntry> auditEntries,
    IRepository<IAccountDbContext, UserNotificationState> states,
    IAccountDbContext dbContext)
    : IRequestHandler<DeleteNotificationCommand, Result>
{
    public async Task<Result> Handle(DeleteNotificationCommand command, CancellationToken cancellationToken)
    {
        if (command.NotificationId == Guid.Empty)
        {
            return Result.Failure(Error.Validation("Notification id is required."));
        }

        var current = currentUserAccessor.GetRequired();
        var tenantId = TenantId.From(current.TenantId);
        var userId = UserId.From(current.UserId);

        var exists = await auditEntries.AnyAsync(x => x.Id == command.NotificationId && x.TenantId == tenantId && x.UserId == userId, cancellationToken);
        if (!exists)
        {
            return Result.Failure(Error.NotFound("Notification"));
        }

        var state = await states.Query.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.UserId == userId && x.NotificationId == command.NotificationId,
            cancellationToken);
        var isNew = state is null;
        state ??= UserNotificationState.Create(tenantId, userId, command.NotificationId, clock.UtcNow);
        state.Delete(clock.UtcNow);
        if (isNew)
        {
            await states.AddAsync(state, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
