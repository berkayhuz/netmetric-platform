// <copyright file="AuthProfileBootstrapOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Infrastructure.IntegrationEvents;

public sealed class AuthProfileBootstrapOptions
{
    public const string SectionName = "AuthProfileBootstrap";

    public bool Enabled { get; set; } = true;

    public string QueueName { get; set; } = "netmetric.account.auth-user-registered.v1";
}
