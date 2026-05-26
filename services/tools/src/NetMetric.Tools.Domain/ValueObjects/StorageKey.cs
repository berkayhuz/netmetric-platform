// <copyright file="StorageKey.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Tools.Domain.ValueObjects;

public readonly record struct StorageKey
{
    public string Value { get; }

    public StorageKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Storage key is required.", nameof(value));
        }

        if (value.Contains("..", StringComparison.Ordinal) || value.Contains('\\') || value.StartsWith('/'))
        {
            throw new ArgumentException("Storage key contains unsafe path segments.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
