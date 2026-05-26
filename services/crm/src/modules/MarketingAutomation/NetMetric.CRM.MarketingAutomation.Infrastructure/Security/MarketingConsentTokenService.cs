// <copyright file="MarketingConsentTokenService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using NetMetric.CRM.MarketingAutomation.Application.Abstractions;

namespace NetMetric.CRM.MarketingAutomation.Infrastructure.Security;

public static class MarketingConsentTokenPurposes
{
    public const string Consent = "consent";
    public const string Unsubscribe = "unsubscribe";
}

public sealed class MarketingConsentTokenOptions
{
    public const string SectionName = "Crm:MarketingAutomation:ConsentTokens";

    public string? SigningKey { get; set; }

    public int LifetimeMinutes { get; set; } = 7 * 24 * 60;
}

public sealed class MarketingConsentTokenService(
    IOptions<MarketingConsentTokenOptions> options,
    IDistributedCache cache,
    TimeProvider timeProvider) : IMarketingConsentTokenService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const string DevelopmentSigningKey = "netmetric-development-only-marketing-consent-token-key-change-in-production";

    public string Issue(MarketingConsentTokenIssueRequest request)
    {
        var now = timeProvider.GetUtcNow();
        var expiresAt = request.ExpiresAtUtc ?? now.AddMinutes(Math.Max(1, options.Value.LifetimeMinutes));
        var payload = new ConsentTokenPayload(
            1,
            request.TenantId,
            request.EmailAddress.Trim().ToLowerInvariant(),
            NormalizePurpose(request.Purpose),
            request.Source.Trim(),
            string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim(),
            request.DoubleOptInRequired,
            expiresAt,
            Guid.NewGuid().ToString("N"));

        var payloadJson = JsonSerializer.Serialize(payload, JsonOptions);
        var payloadSegment = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
        var signatureSegment = Base64UrlEncode(Sign(payloadSegment));
        return $"{payloadSegment}.{signatureSegment}";
    }

    public async Task<MarketingConsentTokenValidationResult> ValidateAndConsumeAsync(
        Guid tenantId,
        string token,
        string expectedPurpose,
        CancellationToken cancellationToken)
    {
        if (!TryReadToken(token, out var payload))
        {
            return MarketingConsentTokenValidationResult.Invalid("invalid");
        }

        if (payload.Version != 1 ||
            payload.TenantId != tenantId ||
            !string.Equals(payload.Purpose, NormalizePurpose(expectedPurpose), StringComparison.Ordinal) ||
            string.IsNullOrWhiteSpace(payload.EmailAddress) ||
            string.IsNullOrWhiteSpace(payload.Source) ||
            string.IsNullOrWhiteSpace(payload.TokenId))
        {
            return MarketingConsentTokenValidationResult.Invalid("invalid");
        }

        if (payload.ExpiresAtUtc < timeProvider.GetUtcNow())
        {
            return MarketingConsentTokenValidationResult.Invalid("expired");
        }

        var replayKey = $"crm:marketing-consent-token:{tenantId:N}:{payload.Purpose}:{payload.TokenId}";
        if (await cache.GetStringAsync(replayKey, cancellationToken) is not null)
        {
            return MarketingConsentTokenValidationResult.Invalid("replay");
        }

        await cache.SetStringAsync(
            replayKey,
            "used",
            new DistributedCacheEntryOptions { AbsoluteExpiration = payload.ExpiresAtUtc },
            cancellationToken);

        return MarketingConsentTokenValidationResult.Valid(
            payload.EmailAddress,
            payload.Source,
            payload.Status,
            payload.DoubleOptInRequired);
    }

    private bool TryReadToken(string token, out ConsentTokenPayload payload)
    {
        payload = default!;
        var parts = token.Split('.', 2);
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
        {
            return false;
        }

        try
        {
            var expectedSignature = Sign(parts[0]);
            if (!CryptographicOperations.FixedTimeEquals(expectedSignature, Base64UrlDecode(parts[1])))
            {
                return false;
            }

            payload = JsonSerializer.Deserialize<ConsentTokenPayload>(
                Encoding.UTF8.GetString(Base64UrlDecode(parts[0])),
                JsonOptions)!;
            return payload is not null;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private byte[] Sign(string payloadSegment)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(GetSigningKey()));
        return hmac.ComputeHash(Encoding.ASCII.GetBytes(payloadSegment));
    }

    private string GetSigningKey()
        => string.IsNullOrWhiteSpace(options.Value.SigningKey)
            ? DevelopmentSigningKey
            : options.Value.SigningKey;

    private static string NormalizePurpose(string purpose)
        => purpose.Trim().ToLowerInvariant();

    private static string Base64UrlEncode(byte[] bytes)
        => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + ((4 - (padded.Length % 4)) % 4), '=');
        return Convert.FromBase64String(padded);
    }

    private sealed record ConsentTokenPayload(
        int Version,
        Guid TenantId,
        string EmailAddress,
        string Purpose,
        string Source,
        string? Status,
        bool DoubleOptInRequired,
        DateTimeOffset ExpiresAtUtc,
        string TokenId);
}
