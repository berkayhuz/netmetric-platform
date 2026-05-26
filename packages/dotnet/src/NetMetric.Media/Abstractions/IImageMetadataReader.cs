// <copyright file="IImageMetadataReader.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Media.Models;

namespace NetMetric.Media.Abstractions;

public interface IImageMetadataReader
{
    Task<ImageMetadata> ReadAsync(Stream content, CancellationToken cancellationToken);
}
