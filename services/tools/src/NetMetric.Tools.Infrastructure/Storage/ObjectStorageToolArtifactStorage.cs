// <copyright file="ObjectStorageToolArtifactStorage.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Tools.Application.Abstractions.Storage;

namespace NetMetric.Tools.Infrastructure.Storage;

public sealed class ObjectStorageToolArtifactStorage : IToolArtifactStorage
{
    public Task PutAsync(ToolArtifactWriteRequest request)
        => throw new InvalidOperationException("Object storage provider is configured but not implemented. Configure a provider integration for production.");

    public Task<ToolArtifactReadResult?> GetAsync(string storageKey, CancellationToken cancellationToken)
        => throw new InvalidOperationException("Object storage provider is configured but not implemented.");

    public Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken)
        => throw new InvalidOperationException("Object storage provider is configured but not implemented.");

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
        => throw new InvalidOperationException("Object storage provider is configured but not implemented.");
}
