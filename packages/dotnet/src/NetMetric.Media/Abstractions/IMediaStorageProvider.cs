// <copyright file="IMediaStorageProvider.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Media.Abstractions;

public interface IMediaStorageProvider
{
    string Name { get; }
    Task SaveAsync(string key, Stream content, string contentType, CancellationToken cancellationToken);
    Task DeleteAsync(string key, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken);
}
