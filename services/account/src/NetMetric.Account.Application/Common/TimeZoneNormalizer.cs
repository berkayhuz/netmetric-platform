// <copyright file="TimeZoneNormalizer.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Application.Common;

internal static class TimeZoneNormalizer
{
    public const string DefaultTimeZone = "UTC";

    public static string NormalizeOrDefault(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? DefaultTimeZone : value.Trim();
        return IsValid(normalized) ? normalized : DefaultTimeZone;
    }

    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(value.Trim());
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            return false;
        }
    }
}
