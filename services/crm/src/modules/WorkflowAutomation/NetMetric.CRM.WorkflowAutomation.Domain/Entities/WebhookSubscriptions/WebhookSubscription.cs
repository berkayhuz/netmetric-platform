// <copyright file="WebhookSubscription.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.WorkflowAutomation.Domain.Entities.WebhookSubscriptions;

public sealed class WebhookSubscription : AuditableEntity
{
    private WebhookSubscription()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string EventKey { get; private set; } = string.Empty;
    public string TargetUrl { get; private set; } = string.Empty;
    public string SecretKeyReference { get; private set; } = string.Empty;
    public string SigningAlgorithm { get; private set; } = "hmac-sha256";
    public int MaxAttempts { get; private set; } = 3;
    public DateTime OccurredAtUtc { get; private set; } = DateTime.UtcNow;

    public static WebhookSubscription Create(
        string name,
        string eventKey,
        string targetUrl,
        string secretKeyReference,
        int maxAttempts = 3)
    {
        return new WebhookSubscription
        {
            Name = Guard.AgainstNullOrWhiteSpace(name),
            EventKey = Guard.AgainstNullOrWhiteSpace(eventKey),
            TargetUrl = Guard.AgainstNullOrWhiteSpace(targetUrl),
            SecretKeyReference = Guard.AgainstNullOrWhiteSpace(secretKeyReference),
            MaxAttempts = Math.Clamp(maxAttempts, 1, 10),
            OccurredAtUtc = DateTime.UtcNow
        };
    }
}
