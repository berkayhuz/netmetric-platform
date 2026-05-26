// <copyright file="AmazonSesEmailProviderOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace NetMetric.Notification.Infrastructure.Modules.Email.Infrastructure.Options;

public sealed class AmazonSesEmailProviderOptions
{
    public const string SectionName = "Notification:Email:AmazonSes";

    [Required]
    public string Region { get; init; } = "eu-central-1";

    [Required]
    [EmailAddress]
    public string FromAddress { get; init; } = string.Empty;

    [Required]
    public string FromName { get; init; } = "NetMetric";

    public string? AccessKeyId { get; init; }
    public string? SecretAccessKey { get; init; }
    public string? EndpointUrl { get; init; }
    public string? ConfigurationSetName { get; init; }
}
