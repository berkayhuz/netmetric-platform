// <copyright file="DefaultNotificationChannelPolicy.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Logging;
using NetMetric.Notification.Application.Abstractions;
using NetMetric.Notification.Contracts.Notifications.Enums;
using NetMetric.Notification.Contracts.Notifications.Requests;

namespace NetMetric.Notification.Application.Services;

public sealed class DefaultNotificationChannelPolicy(
    IUserNotificationPreferenceReader preferenceReader,
    ILogger<DefaultNotificationChannelPolicy> logger) : INotificationChannelPolicy
{
    public async Task<IReadOnlyCollection<NotificationChannel>> ResolveChannelsAsync(
        SendNotificationRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Category == NotificationCategory.Security)
        {
            return request.Channels.Distinct().ToArray();
        }

        if (request.Recipient.UserId is null)
        {
            return request.Channels.Distinct().ToArray();
        }

        var enabledChannels = new List<NotificationChannel>();
        foreach (var channel in request.Channels.Distinct())
        {
            if (await preferenceReader.IsChannelEnabledAsync(
                    request.TenantId,
                    request.Recipient.UserId.Value,
                    request.Category,
                    channel,
                    cancellationToken))
            {
                enabledChannels.Add(channel);
            }
        }

        if (enabledChannels.Count == 0 && request.Channels.Contains(NotificationChannel.InApp))
        {
            logger.LogInformation(
                "All notification channels disabled by preferences; falling back to InApp. UserId={UserId} Category={Category}",
                request.Recipient.UserId,
                request.Category);
            enabledChannels.Add(NotificationChannel.InApp);
        }

        return enabledChannels;
    }
}
