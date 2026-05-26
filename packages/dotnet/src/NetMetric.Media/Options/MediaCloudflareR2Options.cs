// <copyright file="MediaCloudflareR2Options.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Media.Options;

public sealed class MediaCloudflareR2Options
{
    public string AccountId { get; init; } = string.Empty;
    public string BucketName { get; init; } = string.Empty;
    public string AccessKeyId { get; init; } = string.Empty;
    public string SecretAccessKey { get; init; } = string.Empty;
    public string EndpointUrl { get; init; } = string.Empty;
    public string ObjectKeyPrefix { get; init; } = "netmetric";
    public bool UsePathStyle { get; init; } = true;
}
