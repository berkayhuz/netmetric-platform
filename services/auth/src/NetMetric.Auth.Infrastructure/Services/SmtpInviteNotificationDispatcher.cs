// <copyright file="SmtpInviteNotificationDispatcher.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using NetMetric.Auth.Application.Abstractions;
using NetMetric.Auth.Application.Options;
using NetMetric.Auth.Domain.Entities;

namespace NetMetric.Auth.Infrastructure.Services;

public sealed class SmtpInviteNotificationDispatcher(
    IOptions<InvitationDeliveryOptions> options,
    ILogger<SmtpInviteNotificationDispatcher> logger) : IInviteNotificationDispatcher
{
    public Task SendInviteCreatedAsync(
        Tenant tenant,
        TenantInvitation invitation,
        User inviter,
        User? invitedUser,
        string rawToken,
        string? correlationId,
        string? traceId,
        DateTime utcNow,
        CancellationToken cancellationToken) =>
        SendAsync(tenant, invitation, inviter, rawToken, "created", correlationId, cancellationToken);

    public Task SendInviteResentAsync(
        Tenant tenant,
        TenantInvitation invitation,
        User inviter,
        User? invitedUser,
        string rawToken,
        string? correlationId,
        string? traceId,
        DateTime utcNow,
        CancellationToken cancellationToken) =>
        SendAsync(tenant, invitation, inviter, rawToken, "resent", correlationId, cancellationToken);

    private async Task SendAsync(
        Tenant tenant,
        TenantInvitation invitation,
        User inviter,
        string rawToken,
        string deliveryReason,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        var value = options.Value;
        var invitationUrl = value.BuildAcceptUrl(tenant.Id, rawToken, invitation.Email);
        var invitedBy = DisplayName(inviter);
        var subject = $"You're invited to {tenant.Name} on NetMetric";
        var textBody =
            $"{invitedBy} invited you to {tenant.Name} on NetMetric.{Environment.NewLine}{Environment.NewLine}" +
            $"Accept your invite: {invitationUrl}{Environment.NewLine}" +
            $"This invite expires at {invitation.ExpiresAtUtc:O}.";
        var htmlBody =
            $"<p>{HtmlEncode(invitedBy)} invited you to <strong>{HtmlEncode(tenant.Name)}</strong> on NetMetric.</p>" +
            $"<p><a href=\"{HtmlEncode(invitationUrl)}\">Accept your invite</a></p>" +
            $"<p>This invite expires at {invitation.ExpiresAtUtc:O}.</p>";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(value.SenderName, value.SenderAddress));
        message.To.Add(MailboxAddress.Parse(invitation.Email));
        message.Subject = subject;
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            message.Headers.Add("X-Correlation-Id", correlationId);
        }

        message.Headers.Add("X-NetMetric-Invitation-Id", invitation.Id.ToString("D"));
        message.Body = new BodyBuilder
        {
            TextBody = textBody,
            HtmlBody = htmlBody
        }.ToMessageBody();

        using var client = new SmtpClient();
        var socketOptions = value.SmtpUseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
        await client.ConnectAsync(value.SmtpHost!, value.SmtpPort, socketOptions, cancellationToken);
        if (!string.IsNullOrWhiteSpace(value.SmtpUserName))
        {
            await client.AuthenticateAsync(value.SmtpUserName, value.SmtpPassword!, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);

        logger.LogInformation(
            "Invite email sent. TenantId={TenantId} InvitationId={InvitationId} DeliveryReason={DeliveryReason} CorrelationId={CorrelationId}",
            tenant.Id,
            invitation.Id,
            deliveryReason,
            correlationId);
    }

    private static string DisplayName(User user)
    {
        var value = string.Join(' ', new[] { user.FirstName, user.LastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
        return string.IsNullOrWhiteSpace(value) ? user.Email ?? user.UserName : value;
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
