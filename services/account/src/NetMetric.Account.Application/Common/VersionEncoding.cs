// <copyright file="VersionEncoding.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Application.Common;

public static class VersionEncoding
{
    public static string Encode(byte[] version) => Convert.ToBase64String(version);

    public static byte[]? TryDecode(string? version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return null;
        }

        try
        {
            return Convert.FromBase64String(version);
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
