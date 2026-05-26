// <copyright file="WorkflowAutomationOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Processing;

public sealed class WorkflowAutomationOptions
{
    public bool EngineEnabled { get; set; } = true;
    public bool WorkerEnabled { get; set; }
    public int BatchSize { get; set; } = 10;
    public int PollIntervalSeconds { get; set; } = 30;
    public int LeaseSeconds { get; set; } = 300;
    public int BaseRetryDelaySeconds { get; set; } = 30;
    public int MaxRetryDelaySeconds { get; set; } = 900;
    public int MaxAttempts { get; set; } = 3;
    public bool AllowHttpWebhookTargets { get; set; } = true;
    public int WebhookRequestTimeoutSeconds { get; set; } = 30;
    public int WebhookMaxResponseBytes { get; set; } = 65536;
    public bool UseWebhookProxy { get; set; }
    public bool StrictWebhookConnectionPinning { get; set; } = true;
    public string[] WebhookAllowedHosts { get; set; } = [];

    public TimeSpan PollInterval => TimeSpan.FromSeconds(Math.Max(5, PollIntervalSeconds));
    public TimeSpan LeaseDuration => TimeSpan.FromSeconds(Math.Max(30, LeaseSeconds));
    public TimeSpan BaseRetryDelay => TimeSpan.FromSeconds(Math.Max(1, BaseRetryDelaySeconds));
    public TimeSpan MaxRetryDelay => TimeSpan.FromSeconds(Math.Max(BaseRetryDelaySeconds, MaxRetryDelaySeconds));
    public TimeSpan WebhookRequestTimeout => TimeSpan.FromSeconds(Math.Clamp(WebhookRequestTimeoutSeconds, 1, 300));
    public int SafeWebhookMaxResponseBytes => Math.Clamp(WebhookMaxResponseBytes, 1024, 1048576);
}
