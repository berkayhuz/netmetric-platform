// <copyright file="UpdateMyNotificationPreferencesCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Account.Application.Abstractions.Audit;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Common;
using NetMetric.Account.Application.Notifications.Queries;
using NetMetric.Account.Contracts.Notifications;
using NetMetric.Account.Domain.Common;
using NetMetric.Account.Domain.Notifications;
using NetMetric.Clock;

namespace NetMetric.Account.Application.Notifications.Commands;

public sealed record UpdateMyNotificationPreferencesCommand(UpdateNotificationPreferencesRequest Request)
    : IRequest<Result<NotificationPreferencesResponse>>;

public sealed class UpdateMyNotificationPreferencesCommandValidator : AbstractValidator<UpdateMyNotificationPreferencesCommand>
{
    public UpdateMyNotificationPreferencesCommandValidator()
    {
        RuleFor(x => x.Request.Items).NotNull().NotEmpty();
        RuleForEach(x => x.Request.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Channel).Must(value => Enum.TryParse<NotificationChannel>(value, true, out _));
            item.RuleFor(x => x.Category).Must(value => Enum.TryParse<NotificationCategory>(value, true, out _));
        });
    }
}

public sealed class UpdateMyNotificationPreferencesCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IClock clock,
    IRepository<IAccountDbContext, NotificationPreference> notificationPreferences,
    IAccountDbContext dbContext,
    IAccountAuditWriter auditWriter)
    : IRequestHandler<UpdateMyNotificationPreferencesCommand, Result<NotificationPreferencesResponse>>
{
    public async Task<Result<NotificationPreferencesResponse>> Handle(
        UpdateMyNotificationPreferencesCommand command,
        CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var tenantId = TenantId.From(currentUser.TenantId);
        var userId = UserId.From(currentUser.UserId);

        var existing = await notificationPreferences.Query
            .Where(x => x.TenantId == tenantId && x.UserId == userId)
            .ToListAsync(cancellationToken);

        foreach (var item in command.Request.Items)
        {
            var channel = Enum.Parse<NotificationChannel>(item.Channel, true);
            var category = Enum.Parse<NotificationCategory>(item.Category, true);
            var preference = existing.FirstOrDefault(x => x.Channel == channel && x.Category == category);

            if (preference is null)
            {
                preference = NotificationPreference.Create(tenantId, userId, channel, category, item.IsEnabled, clock.UtcNow);
                await notificationPreferences.AddAsync(preference, cancellationToken);
                existing.Add(preference);
            }
            else
            {
                preference.SetEnabled(item.IsEnabled, clock.UtcNow);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditWriter.WriteAsync(
            new AccountAuditWriteRequest(
                currentUser.TenantId,
                currentUser.UserId,
                AccountAuditEventTypes.NotificationPreferencesUpdated,
                "Information",
                currentUser.CorrelationId,
                currentUser.IpAddress,
                currentUser.UserAgent,
                null),
            cancellationToken);

        return Result<NotificationPreferencesResponse>.Success(NotificationPreferenceMapper.ToResponse(existing));
    }
}
