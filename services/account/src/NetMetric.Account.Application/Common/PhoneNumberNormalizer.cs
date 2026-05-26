// <copyright file="PhoneNumberNormalizer.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using PhoneNumbers;

namespace NetMetric.Account.Application.Common;

internal static class PhoneNumberNormalizer
{
    public static string? Normalize(string? iso2, string? nationalNumber)
    {
        if (string.IsNullOrWhiteSpace(iso2) && string.IsNullOrWhiteSpace(nationalNumber))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(iso2) || string.IsNullOrWhiteSpace(nationalNumber))
        {
            return null;
        }

        var phoneUtil = PhoneNumberUtil.GetInstance();
        try
        {
            var parsed = phoneUtil.Parse(nationalNumber.Trim(), iso2.Trim().ToUpperInvariant());
            if (!phoneUtil.IsValidNumberForRegion(parsed, iso2.Trim().ToUpperInvariant()))
            {
                return null;
            }

            return phoneUtil.Format(parsed, PhoneNumberFormat.E164);
        }
        catch (NumberParseException)
        {
            return null;
        }
    }

    public static (string? Iso2, string? CallingCode, string? NationalNumber) Split(string? e164)
    {
        if (string.IsNullOrWhiteSpace(e164))
        {
            return (null, null, null);
        }

        var phoneUtil = PhoneNumberUtil.GetInstance();
        try
        {
            var parsed = phoneUtil.Parse(e164, null);
            var iso2 = phoneUtil.GetRegionCodeForNumber(parsed);
            var national = phoneUtil.GetNationalSignificantNumber(parsed);
            return (iso2, $"+{parsed.CountryCode}", national);
        }
        catch (NumberParseException)
        {
            return (null, null, null);
        }
    }
}
