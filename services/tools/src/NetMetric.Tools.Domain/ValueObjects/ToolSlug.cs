// <copyright file="ToolSlug.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Tools.Domain.ValueObjects;

public readonly record struct ToolSlug
{
    public string Value { get; }

    public ToolSlug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Tool slug is required.", nameof(value));
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (!System.Text.RegularExpressions.Regex.IsMatch(normalized, "^[a-z0-9]+(?:-[a-z0-9]+)*$"))
        {
            throw new ArgumentException("Tool slug must be kebab-case.", nameof(value));
        }

        Value = normalized;
    }

    public override string ToString() => Value;

    public static implicit operator string(ToolSlug value) => value.Value;
    public static explicit operator ToolSlug(string value) => new(value);
}
