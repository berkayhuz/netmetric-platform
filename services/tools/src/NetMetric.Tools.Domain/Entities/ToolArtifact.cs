// <copyright file="ToolArtifact.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Tools.Domain.ValueObjects;

namespace NetMetric.Tools.Domain.Entities;

public sealed class ToolArtifact
{
    public Guid Id { get; private set; }
    public Guid ToolRunId { get; private set; }
    public Guid OwnerUserId { get; private set; }
    public string MimeType { get; private set; }
    public long SizeBytes { get; private set; }
    public string StorageKey { get; private set; }
    public string OriginalFileName { get; private set; }
    public string ChecksumSha256 { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? ExpiresAtUtc { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }

    private ToolArtifact()
    {
        MimeType = string.Empty;
        StorageKey = string.Empty;
        OriginalFileName = string.Empty;
        ChecksumSha256 = string.Empty;
    }

    public ToolArtifact(
        Guid toolRunId,
        OwnerUserId ownerUserId,
        MimeType mimeType,
        FileSizeBytes size,
        StorageKey storageKey,
        string originalFileName,
        string checksumSha256,
        DateTimeOffset? expiresAtUtc = null)
    {
        if (toolRunId == Guid.Empty) throw new ArgumentException("Tool run id is required.", nameof(toolRunId));
        if (string.IsNullOrWhiteSpace(originalFileName)) throw new ArgumentException("Original file name is required.", nameof(originalFileName));
        if (string.IsNullOrWhiteSpace(checksumSha256)) throw new ArgumentException("Checksum is required.", nameof(checksumSha256));

        Id = ToolArtifactId.New().Value;
        ToolRunId = toolRunId;
        OwnerUserId = ownerUserId.Value;
        MimeType = mimeType.Value;
        SizeBytes = size.Value;
        StorageKey = storageKey.Value;
        OriginalFileName = originalFileName.Trim();
        ChecksumSha256 = checksumSha256.Trim();
        CreatedAtUtc = DateTimeOffset.UtcNow;
        ExpiresAtUtc = expiresAtUtc;
    }

    public void SoftDelete() => DeletedAtUtc = DateTimeOffset.UtcNow;
}
