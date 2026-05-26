// <copyright file="EmailProviderOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace NetMetric.Notification.Infrastructure.Modules.Email.Infrastructure.Options;

public sealed class EmailProviderOptions
{
    public const string SectionName = "Notification:Email";

    [Required]
    public string Provider { get; init; } = "smtp";
}
