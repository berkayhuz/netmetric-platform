// <copyright file="TrustedGatewayRequestValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NetMetric.AspNetCore.Security;
using NetMetric.AspNetCore.TrustedGateway.Abstractions;
using NetMetric.AspNetCore.TrustedGateway.Models;
using NetMetric.AspNetCore.TrustedGateway.Options;
using NetMetric.AspNetCore.TrustedGateway.Signing;

namespace NetMetric.AspNetCore.TrustedGateway.Validation;

public sealed class TrustedGatewayRequestValidator(
    TrustedGatewayOptions options,
    ITrustedGatewayReplayProtector replayProtector,
    ILogger<TrustedGatewayRequestValidator> logger) : ITrustedGatewayRequestValidator
{
    public async Task<TrustedGatewayValidationResult> ValidateAsync(HttpContext context, string correlationId, CancellationToken cancellationToken)
    {
        var keyId = context.Request.Headers[options.KeyIdHeaderName].FirstOrDefault();
        var signature = context.Request.Headers[options.SignatureHeaderName].FirstOrDefault();
        var timestamp = context.Request.Headers[options.TimestampHeaderName].FirstOrDefault();
        var source = context.Request.Headers[options.SourceHeaderName].FirstOrDefault();
        var nonce = context.Request.Headers[options.NonceHeaderName].FirstOrDefault();
        var contentHash = context.Request.Headers[options.ContentHashHeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(keyId) ||
            string.IsNullOrWhiteSpace(signature) ||
            string.IsNullOrWhiteSpace(timestamp) ||
            string.IsNullOrWhiteSpace(source) ||
            string.IsNullOrWhiteSpace(nonce) ||
            string.IsNullOrWhiteSpace(contentHash))
        {
            return TrustedGatewayValidationResult.Fail(TrustedGatewayFailureReason.MissingHeaders, "trusted_gateway_missing_headers");
        }

        if (!options.AllowedSources.Contains(source, StringComparer.Ordinal))
        {
            return TrustedGatewayValidationResult.Fail(TrustedGatewayFailureReason.InvalidSource, "trusted_gateway_invalid_source");
        }

        if (options.AllowedGatewayProxies.Length > 0 || options.AllowedGatewayNetworks.Length > 0)
        {
            var remoteAddress = context.Connection.RemoteIpAddress;
            if (!SecuritySupport.IsAllowedRemoteAddress(remoteAddress, options.AllowedGatewayProxies, options.AllowedGatewayNetworks))
            {
                return TrustedGatewayValidationResult.Fail(TrustedGatewayFailureReason.InvalidRemoteAddress, "trusted_gateway_invalid_remote_address");
            }
        }

        var key = options.Keys.FirstOrDefault(x => x.Enabled && string.Equals(x.KeyId, keyId, StringComparison.Ordinal));
        if (key is null || string.IsNullOrWhiteSpace(key.Secret))
        {
            return TrustedGatewayValidationResult.Fail(TrustedGatewayFailureReason.InvalidKey, "trusted_gateway_invalid_key");
        }

        if (!long.TryParse(timestamp, NumberStyles.None, CultureInfo.InvariantCulture, out var unixSeconds))
        {
            return TrustedGatewayValidationResult.Fail(TrustedGatewayFailureReason.InvalidTimestamp, "trusted_gateway_invalid_timestamp");
        }

        var timestampUtc = DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime;
        var skewSeconds = Math.Abs((DateTime.UtcNow - timestampUtc).TotalSeconds);
        if (skewSeconds > options.AllowedClockSkewSeconds)
        {
            return TrustedGatewayValidationResult.Fail(TrustedGatewayFailureReason.TimestampSkewExceeded, "trusted_gateway_timestamp_skew");
        }

        var computedBodyHash = await TrustedGatewayCanonicalRequest.ComputeBodyHashAsync(context.Request, cancellationToken);
        if (!FixedTimeHexEquals(contentHash, computedBodyHash))
        {
            logger.LogWarning("Trusted gateway content hash mismatch for {Path}.", context.Request.Path);
            return TrustedGatewayValidationResult.Fail(TrustedGatewayFailureReason.InvalidContentHash, "trusted_gateway_invalid_content_hash");
        }

        var canonical = await TrustedGatewayCanonicalRequest.BuildAsync(
            context.Request,
            timestamp,
            nonce,
            correlationId,
            source,
            contentHash,
            cancellationToken);
        var computedSignature = ComputeSignature(key.Secret, canonical);

        if (!FixedTimeHexEquals(signature, computedSignature))
        {
            logger.LogWarning("Trusted gateway signature mismatch for {Path} using key {KeyId}.", context.Request.Path, keyId);
            return TrustedGatewayValidationResult.Fail(TrustedGatewayFailureReason.InvalidSignature, "trusted_gateway_invalid_signature");
        }

        var replayTtl = TimeSpan.FromSeconds(Math.Max(options.ReplayProtectionWindowSeconds, options.AllowedClockSkewSeconds * 2));
        if (!await replayProtector.TryRegisterAsync(keyId, nonce, replayTtl, cancellationToken))
        {
            return TrustedGatewayValidationResult.Fail(TrustedGatewayFailureReason.ReplayDetected, "trusted_gateway_replay_detected");
        }

        return TrustedGatewayValidationResult.Success();
    }

    private static string ComputeSignature(string secret, string canonical)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(canonical)));
    }

    private static bool FixedTimeHexEquals(string left, string right)
    {
        try
        {
            var leftBytes = Convert.FromHexString(left);
            var rightBytes = Convert.FromHexString(right);
            return leftBytes.Length == rightBytes.Length && CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
