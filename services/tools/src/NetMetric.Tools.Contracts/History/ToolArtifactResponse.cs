// <copyright file="ToolArtifactResponse.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Tools.Contracts.History;

public sealed record ToolArtifactResponse(
    Guid ArtifactId,
    string MimeType,
    long SizeBytes,
    string FileName,
    string ChecksumSha256,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ExpiresAtUtc);
