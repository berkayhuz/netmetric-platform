// <copyright file="MarketingAutomationOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.MarketingAutomation.Infrastructure.Processing;

public sealed class MarketingAutomationOptions
{
    public const string SectionName = "Crm:MarketingAutomation";
    public bool EngineEnabled { get; set; } = true;
    public bool WorkerEnabled { get; set; }
    public bool EmailDeliveryEnabled { get; set; }
    public int BatchSize { get; set; } = 50;
    public int MaxAttempts { get; set; } = 3;
    public int BaseRetryDelaySeconds { get; set; } = 60;
}
