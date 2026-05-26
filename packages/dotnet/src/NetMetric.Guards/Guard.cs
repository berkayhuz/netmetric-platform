// <copyright file="Guard.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Guards;

public static class Guard
{
    public static string AgainstNullOrWhiteSpace(string? value, string? parameterName = null)
        => string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value cannot be null or whitespace.", parameterName ?? nameof(value))
            : value.Trim();

    public static T AgainstNull<T>(T? value, string? parameterName = null)
        where T : class
        => value ?? throw new ArgumentNullException(parameterName ?? nameof(value));

    public static Guid AgainstEmpty(Guid value, string? parameterName = null)
        => value == Guid.Empty ? throw new ArgumentException("Value cannot be empty.", parameterName ?? nameof(value)) : value;
}
