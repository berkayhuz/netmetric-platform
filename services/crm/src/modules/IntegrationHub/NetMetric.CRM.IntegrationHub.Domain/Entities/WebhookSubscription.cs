// <copyright file="WebhookSubscription.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.IntegrationHub.Domain.Entities;

public sealed class WebhookSubscription : EntityBase
{
    public string Name { get; private set; } = null!;
    public string EventKey { get; private set; } = null!;
    public string TargetUrl { get; private set; } = null!;
    public string SecretKey { get; private set; } = null!;
    public bool IsEnabled { get; private set; } = true;
    public int TimeoutSeconds { get; private set; }
    public int MaxAttempts { get; private set; }
    public DateTime? LastSuccessAtUtc { get; private set; }
    public DateTime? LastFailureAtUtc { get; private set; }

    private WebhookSubscription() { }

    public WebhookSubscription(
        Guid tenantId,
        string name,
        string eventKey,
        string targetUrl,
        string secretKey,
        int timeoutSeconds,
        int maxAttempts)
    {
        TenantId = tenantId;
        Name = Guard.AgainstNullOrWhiteSpace(name).Trim();
        EventKey = eventKey.Trim();
        TargetUrl = targetUrl.Trim();
        SecretKey = Guard.AgainstNullOrWhiteSpace(secretKey).Trim();
        TimeoutSeconds = Math.Clamp(timeoutSeconds, 1, 30);
        MaxAttempts = Math.Clamp(maxAttempts, 1, 10);
    }

    public void Reconfigure(string name, string eventKey, string targetUrl, string secretKey, bool isEnabled, int timeoutSeconds, int maxAttempts)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name).Trim();
        EventKey = Guard.AgainstNullOrWhiteSpace(eventKey).Trim();
        TargetUrl = Guard.AgainstNullOrWhiteSpace(targetUrl).Trim();
        SecretKey = Guard.AgainstNullOrWhiteSpace(secretKey).Trim();
        IsEnabled = isEnabled;
        TimeoutSeconds = Math.Clamp(timeoutSeconds, 1, 30);
        MaxAttempts = Math.Clamp(maxAttempts, 1, 10);
    }

    public void Disable() => IsEnabled = false;

    public void MarkDeliveryResult(bool success, DateTime timestampUtc)
    {
        if (success)
        {
            LastSuccessAtUtc = timestampUtc;
            return;
        }

        LastFailureAtUtc = timestampUtc;
    }
}
