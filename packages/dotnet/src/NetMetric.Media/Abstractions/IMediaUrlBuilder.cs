// <copyright file="IMediaUrlBuilder.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Media.Abstractions;

public interface IMediaUrlBuilder
{
    string BuildPublicUrl(string key);
}
