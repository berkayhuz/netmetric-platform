// <copyright file="MediaUrlBuilder.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Options;
using NetMetric.Media.Abstractions;
using NetMetric.Media.Options;

namespace NetMetric.Media.Urls;

public sealed class MediaUrlBuilder(IOptions<MediaOptions> options) : IMediaUrlBuilder
{
    public string BuildPublicUrl(string key)
    {
        var baseUrl = options.Value.PublicBaseUrl?.TrimEnd('/') ?? string.Empty;
        var safeKey = key.TrimStart('/');
        return $"{baseUrl}/{safeKey}";
    }
}
