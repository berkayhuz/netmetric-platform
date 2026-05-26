// <copyright file="HmacIntegrationWebhookSecurityService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Processing;

namespace NetMetric.CRM.IntegrationHub.Infrastructure.Webhooks;

public sealed class HmacIntegrationWebhookSecurityService : IIntegrationWebhookSecurityService
{
    public bool ValidateSignature(
        string secret,
        string payload,
        string timestamp,
        string signature,
        DateTime nowUtc,
        TimeSpan tolerance)
    {
        if (string.IsNullOrWhiteSpace(secret) ||
            string.IsNullOrWhiteSpace(timestamp) ||
            string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        if (!long.TryParse(timestamp, NumberStyles.None, CultureInfo.InvariantCulture, out var unixSeconds))
        {
            return false;
        }

        var signedAt = DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime;
        if (signedAt < nowUtc.Subtract(tolerance) || signedAt > nowUtc.Add(tolerance))
        {
            return false;
        }

        var normalizedSignature = signature.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase)
            ? signature["sha256=".Length..]
            : signature;

        var material = $"{timestamp}.{payload}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var expected = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(material))).ToLowerInvariant();
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(normalizedSignature.ToLowerInvariant()));
    }

    public string ComputePayloadHash(string payload)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload ?? string.Empty))).ToLowerInvariant();

    public string ComputeSignatureHash(string signature)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(signature ?? string.Empty))).ToLowerInvariant();

    public string CreateSignature(string secret, string payload, string timestamp)
    {
        var material = $"{timestamp}.{payload}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret ?? string.Empty));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(material))).ToLowerInvariant();
    }
}
