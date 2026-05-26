// <copyright file="NotificationIntegrationConsumerOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace NetMetric.Notification.Infrastructure.Integration;

public sealed class NotificationIntegrationConsumerOptions
{
    public const string SectionName = "Notification:IntegrationConsumer";

    public bool Enabled { get; init; } = true;

    [Required]
    public string QueueName { get; init; } = "netmetric.notification.integration.v1";
}
