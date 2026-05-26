// <copyright file="SecurityAlertOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Application.Options;

public sealed class SecurityAlertOptions
{
    public const string SectionName = "Security:Alerts";

    public bool EnableStructuredAlerts { get; set; } = true;
}
