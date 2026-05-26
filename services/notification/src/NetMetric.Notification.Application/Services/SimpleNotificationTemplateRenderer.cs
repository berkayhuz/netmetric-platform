// <copyright file="SimpleNotificationTemplateRenderer.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.RegularExpressions;
using NetMetric.Localization;
using NetMetric.Notification.Application.Abstractions;
using NetMetric.Notification.Contracts.Notifications.Requests;

namespace NetMetric.Notification.Application.Services;

public sealed partial class SimpleNotificationTemplateRenderer : INotificationTemplateRenderer
{
    public SendNotificationRequest Render(SendNotificationRequest request)
    {
        var values = request.Metadata;
        var culture = ResolveCulture(values);
        var template = LocalizedNotificationTemplates.Resolve(request.TemplateKey, culture);
        var subject = Bind(template?.Subject ?? request.Subject, values);
        var textBody = Bind(template?.TextBody ?? request.TextBody, values);
        var htmlBody = template?.HtmlBody is null && request.HtmlBody is null
            ? null
            : Bind(template?.HtmlBody ?? request.HtmlBody!, values);

        return request with
        {
            Subject = subject,
            TextBody = textBody,
            HtmlBody = htmlBody
        };
    }

    private static string Bind(string template, IReadOnlyDictionary<string, string> values) =>
        PlaceholderRegex().Replace(template, match =>
        {
            var key = match.Groups["key"].Value;
            return values.TryGetValue(key, out var value) ? value : match.Value;
        });

    [GeneratedRegex("\\{\\{(?<key>[a-zA-Z0-9_.-]+)\\}\\}", RegexOptions.CultureInvariant)]
    private static partial Regex PlaceholderRegex();

    private static string ResolveCulture(IReadOnlyDictionary<string, string> metadata)
        => metadata.TryGetValue("culture", out var culture) || metadata.TryGetValue("Culture", out culture)
            ? NetMetricCultures.NormalizeOrDefault(culture)
            : NetMetricCultures.DefaultCulture;
}

internal sealed record LocalizedNotificationTemplate(string Subject, string TextBody, string HtmlBody);

internal static class LocalizedNotificationTemplates
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, LocalizedNotificationTemplate>> Templates =
        new Dictionary<string, IReadOnlyDictionary<string, LocalizedNotificationTemplate>>(StringComparer.OrdinalIgnoreCase)
        {
            ["auth.email-confirmation"] = Pair(
                "Confirm your NetMetric email",
                "Hello {{DisplayName}}, confirm your email by opening {{ActionUrl}}.",
                "<p>Hello {{DisplayName}},</p><p>Confirm your email by opening <a href=\"{{ActionUrl}}\">this link</a>.</p>",
                "NetMetric e-posta adresini dogrula",
                "Merhaba {{DisplayName}}, e-posta adresini dogrulamak icin {{ActionUrl}} baglantisini ac.",
                "<p>Merhaba {{DisplayName}},</p><p>E-posta adresini dogrulamak icin <a href=\"{{ActionUrl}}\">bu baglantiyi</a> ac.</p>"),
            ["auth.password-reset"] = Pair(
                "Reset your NetMetric password",
                "Reset your password by opening {{ActionUrl}}.",
                "<p>Reset your password by opening <a href=\"{{ActionUrl}}\">this link</a>.</p>",
                "NetMetric parolani sifirla",
                "Parolani sifirlamak icin {{ActionUrl}} baglantisini ac.",
                "<p>Parolani sifirlamak icin <a href=\"{{ActionUrl}}\">bu baglantiyi</a> ac.</p>"),
            ["auth.email-change-confirmation"] = Pair(
                "Confirm your new email address",
                "Confirm this email change by opening {{ActionUrl}}.",
                "<p>Confirm this email change by opening <a href=\"{{ActionUrl}}\">this link</a>.</p>",
                "Yeni e-posta adresini dogrula",
                "Bu e-posta degisikligini dogrulamak icin {{ActionUrl}} baglantisini ac.",
                "<p>Bu e-posta degisikligini dogrulamak icin <a href=\"{{ActionUrl}}\">bu baglantiyi</a> ac.</p>"),
            ["account.security"] = Pair(
                "NetMetric security notification",
                "A security event occurred on your account: {{Reason}}.",
                "<p>A security event occurred on your account: {{Reason}}.</p>",
                "NetMetric guvenlik bildirimi",
                "Hesabinda bir guvenlik olayi gerceklesti: {{Reason}}.",
                "<p>Hesabinda bir guvenlik olayi gerceklesti: {{Reason}}.</p>"),
            ["crm.reminder"] = Pair(
                "CRM reminder",
                "Reminder: {{Title}}.",
                "<p>Reminder: {{Title}}.</p>",
                "CRM hatirlaticisi",
                "Hatirlatma: {{Title}}.",
                "<p>Hatirlatma: {{Title}}.</p>"),
            ["task.notification"] = Pair(
                "Task notification",
                "Task update: {{Title}}.",
                "<p>Task update: {{Title}}.</p>",
                "Gorev bildirimi",
                "Gorev guncellemesi: {{Title}}.",
                "<p>Gorev guncellemesi: {{Title}}.</p>")
        };

    public static LocalizedNotificationTemplate? Resolve(string? templateKey, string culture)
        => templateKey is not null && Templates.TryGetValue(templateKey, out var localized)
            ? localized.TryGetValue(NetMetricCultures.NormalizeOrDefault(culture), out var template)
                ? template
                : localized[NetMetricCultures.DefaultCulture]
            : null;

    private static IReadOnlyDictionary<string, LocalizedNotificationTemplate> Pair(
        string enSubject,
        string enText,
        string enHtml,
        string trSubject,
        string trText,
        string trHtml) =>
        new Dictionary<string, LocalizedNotificationTemplate>(StringComparer.OrdinalIgnoreCase)
        {
            [NetMetricCultures.EnglishCulture] = new(enSubject, enText, enHtml),
            [NetMetricCultures.TurkishCulture] = new(trSubject, trText, trHtml)
        };
}
