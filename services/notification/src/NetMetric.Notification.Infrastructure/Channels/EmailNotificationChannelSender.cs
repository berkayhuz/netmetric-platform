// <copyright file="EmailNotificationChannelSender.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Notification.Application.Abstractions;
using NetMetric.Notification.Contracts.Notifications.Enums;
using NetMetric.Notification.Contracts.Notifications.Requests;
using NetMetric.Notification.Infrastructure.Modules.Email.Application;

namespace NetMetric.Notification.Infrastructure.Channels;

public sealed class EmailNotificationChannelSender(IEmailProvider emailProvider) : INotificationChannelSender
{
    public NotificationChannel Channel => NotificationChannel.Email;
    public string ProviderName => emailProvider.Name;

    public async Task<NotificationChannelSendResult> SendAsync(
        SendNotificationRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Recipient.EmailAddress))
        {
            return new NotificationChannelSendResult(
                Succeeded: false,
                ExternalMessageId: null,
                ErrorCode: "email_recipient_missing",
                ErrorMessage: "Email channel requires recipient email address.");
        }

        var emailMessage = new EmailMessage(
            request.Recipient.EmailAddress,
            request.Subject,
            request.HtmlBody ?? request.TextBody,
            request.TextBody,
            request.CorrelationId);

        await emailProvider.SendAsync(emailMessage, cancellationToken);
        return new NotificationChannelSendResult(true, null, null, null);
    }
}
