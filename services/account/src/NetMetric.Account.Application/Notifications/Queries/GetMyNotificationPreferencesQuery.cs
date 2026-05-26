// <copyright file="GetMyNotificationPreferencesQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Common;
using NetMetric.Account.Contracts.Notifications;
using NetMetric.Account.Domain.Common;
using NetMetric.Account.Domain.Notifications;

namespace NetMetric.Account.Application.Notifications.Queries;

public sealed record GetMyNotificationPreferencesQuery : IRequest<Result<NotificationPreferencesResponse>>;

public sealed class GetMyNotificationPreferencesQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IRepository<IAccountDbContext, NotificationPreference> notificationPreferences)
    : IRequestHandler<GetMyNotificationPreferencesQuery, Result<NotificationPreferencesResponse>>
{
    public async Task<Result<NotificationPreferencesResponse>> Handle(GetMyNotificationPreferencesQuery request, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var tenantId = TenantId.From(currentUser.TenantId);
        var userId = UserId.From(currentUser.UserId);

        var preferences = await notificationPreferences.Query
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == userId)
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Channel)
            .ToListAsync(cancellationToken);

        return Result<NotificationPreferencesResponse>.Success(NotificationPreferenceMapper.ToResponse(preferences));
    }
}

internal static class NotificationPreferenceMapper
{
    public static NotificationPreferencesResponse ToResponse(IReadOnlyCollection<NotificationPreference> preferences)
        => new(preferences
            .Select(preference => new NotificationPreferenceItemResponse(
                preference.Id,
                preference.Channel.ToString(),
                preference.Category.ToString(),
                preference.IsEnabled,
                VersionEncoding.Encode(preference.Version)))
            .ToArray());
}
