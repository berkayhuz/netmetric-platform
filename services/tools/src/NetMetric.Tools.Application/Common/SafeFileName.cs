// <copyright file="SafeFileName.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Tools.Application.Common;

public static class SafeFileName
{
    private static readonly char[] InvalidChars = Path.GetInvalidFileNameChars();

    public static string Normalize(string? fileName)
    {
        var fallback = "artifact.bin";
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return fallback;
        }

        var sanitized = fileName.Trim();
        foreach (var c in InvalidChars)
        {
            sanitized = sanitized.Replace(c, '_');
        }

        sanitized = sanitized.Replace("..", "_").Replace("/", "_").Replace("\\", "_");

        if (string.IsNullOrWhiteSpace(sanitized))
        {
            return fallback;
        }

        return sanitized.Length > 120 ? sanitized[..120] : sanitized;
    }
}
