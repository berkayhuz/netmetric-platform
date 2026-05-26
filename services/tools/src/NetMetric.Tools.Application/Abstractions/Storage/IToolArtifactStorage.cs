// <copyright file="IToolArtifactStorage.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Tools.Application.Abstractions.Storage;

public sealed record ToolArtifactWriteRequest(string StorageKey, string MimeType, string FileName, string ChecksumSha256, Stream Content, CancellationToken CancellationToken);

public sealed record ToolArtifactReadResult(Stream Content, string MimeType, long Length);

public interface IToolArtifactStorage
{
    Task PutAsync(ToolArtifactWriteRequest request);
    Task<ToolArtifactReadResult?> GetAsync(string storageKey, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken);
}
