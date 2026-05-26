// <copyright file="AuthenticationNormalization.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.RegularExpressions;

namespace NetMetric.Auth.Application.Helpers;

internal static partial class AuthenticationNormalization
{
    public static string Normalize(string value) => value.Trim().ToUpperInvariant();

    public static string? CleanOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return MultiSpaceRegex().Replace(value.Trim(), " ");
    }

    [GeneratedRegex("\\s+")]
    private static partial Regex MultiSpaceRegex();
}
