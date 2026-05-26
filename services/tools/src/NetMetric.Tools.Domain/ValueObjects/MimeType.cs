// <copyright file="MimeType.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Tools.Domain.ValueObjects;

public readonly record struct MimeType
{
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    private static readonly HashSet<string> AllowedTypes =
    [
        "image/png",
        "image/jpeg",
        "image/jpg",
        "image/webp",
        "application/pdf",
        "application/json",
        "text/plain"
    ];

    public string Value { get; }

    public MimeType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("MIME type is required.", nameof(value));
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (!AllowedTypes.Contains(normalized, Comparer))
        {
            throw new ArgumentException($"Unsupported MIME type: {value}", nameof(value));
        }

        Value = normalized;
    }

    public override string ToString() => Value;

    public static bool IsAllowed(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return AllowedTypes.Contains(value.Trim(), Comparer);
    }
}
