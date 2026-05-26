// <copyright file="ArtifactRules.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Security.Cryptography;
using NetMetric.Tools.Domain.Entities;
using NetMetric.Tools.Domain.ValueObjects;

namespace NetMetric.Tools.Application.Common;

public static class ArtifactRules
{
    private static readonly HashSet<string> BlockedMimeTypes = ["image/svg+xml"];

    public static void EnsureSaveAllowed(ToolDefinition tool, string mimeType, long sizeBytes)
    {
        if (sizeBytes <= 0)
        {
            throw new InvalidOperationException("Artifact size must be greater than zero.");
        }

        if (sizeBytes > FileSizeBytes.AuthenticatedMaxBytes)
        {
            throw new InvalidOperationException("Artifact exceeds authenticated save size limit (10 MB).");
        }

        if (BlockedMimeTypes.Contains(mimeType.Trim().ToLowerInvariant()))
        {
            throw new InvalidOperationException("SVG uploads are not allowed for this tool.");
        }

        if (!tool.AcceptedMimeTypes.Contains(mimeType.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Artifact MIME type is not allowed for this tool.");
        }
    }

    public static async Task<string> ComputeSha256Async(Stream stream, CancellationToken cancellationToken)
    {
        stream.Position = 0;
        using var hasher = SHA256.Create();
        var hash = await hasher.ComputeHashAsync(stream, cancellationToken);
        stream.Position = 0;
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
