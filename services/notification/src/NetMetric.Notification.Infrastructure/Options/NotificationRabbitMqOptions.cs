// <copyright file="NotificationRabbitMqOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace NetMetric.Notification.Infrastructure.Options;

public sealed class NotificationRabbitMqOptions
{
    public const string SectionName = "Notification:RabbitMq";

    [Required]
    public string Host { get; init; } = "localhost";

    [Range(1, 65535)]
    public int Port { get; init; } = 5672;

    [Required]
    public string Username { get; init; } = "guest";

    [Required]
    public string Password { get; init; } = "guest";

    [Required]
    public string QueueName { get; init; } = "netmetric.notification.dispatch.v1";

    [Required]
    public string DeadLetterExchangeName { get; init; } = "netmetric.notification.dlx";

    [Required]
    public string DeadLetterQueueName { get; init; } = "netmetric.notification.dispatch.dlq.v1";

    [Required]
    public string DeadLetterRoutingKey { get; init; } = "notification.dispatch.dead";

    public bool UseQuorumQueue { get; init; } = true;

    [Range(1, 60)]
    public int NetworkRecoveryIntervalSeconds { get; init; } = 10;

    [Range(1, 128)]
    public ushort PrefetchCount { get; init; } = 8;

    public bool UseTls { get; init; }

    public string? SslServerName { get; init; }
}
