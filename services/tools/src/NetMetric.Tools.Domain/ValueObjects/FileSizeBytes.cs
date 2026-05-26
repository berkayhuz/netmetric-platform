// <copyright file="FileSizeBytes.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Tools.Domain.ValueObjects;

public readonly record struct FileSizeBytes
{
    public const long AuthenticatedMaxBytes = 10L * 1024L * 1024L;
    public const long GuestGuidanceMaxBytes = 5L * 1024L * 1024L;

    public long Value { get; }

    public FileSizeBytes(long value)
    {
        if (value <= 0)
        {
            throw new ArgumentException("File size must be greater than zero.", nameof(value));
        }

        Value = value;
    }

    public override string ToString() => Value.ToString();
}
