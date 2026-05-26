// <copyright file="IdentityInternalRequestSigningHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NetMetric.AspNetCore.RequestContext;
using NetMetric.AspNetCore.TrustedGateway.Options;

namespace NetMetric.Account.Infrastructure.Identity;

public sealed class IdentityInternalRequestSigningHandler(
    IHttpContextAccessor httpContextAccessor,
    IOptions<TrustedGatewayOptions> options) : DelegatingHandler
{
    private static readonly string EmptyBodyHash = Convert.ToHexString(SHA256.HashData(Array.Empty<byte>()));

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var value = options.Value;
        var signingKey = value.Keys.FirstOrDefault(x =>
            x.Enabled &&
            x.SignRequests &&
            string.Equals(x.KeyId, value.CurrentKeyId, StringComparison.Ordinal));

        if (signingKey is null)
        {
            throw new InvalidOperationException("Security:TrustedGateway current signing key is missing or disabled for Account -> Auth calls.");
        }

        var correlationId = ResolveCorrelationId();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        var nonce = Guid.NewGuid().ToString("N");
        var contentHash = await ComputeBodyHashAsync(request, cancellationToken);
        var canonical = await BuildCanonicalRequestAsync(request, timestamp, nonce, correlationId, value.Source, contentHash, cancellationToken);
        var signature = ComputeSignature(signingKey.Secret, canonical);

        request.Headers.Remove(value.SignatureHeaderName);
        request.Headers.Remove(value.TimestampHeaderName);
        request.Headers.Remove(value.KeyIdHeaderName);
        request.Headers.Remove(value.SourceHeaderName);
        request.Headers.Remove(value.NonceHeaderName);
        request.Headers.Remove(value.ContentHashHeaderName);
        request.Headers.Remove(RequestContextSupport.CorrelationIdHeaderName);

        request.Headers.TryAddWithoutValidation(value.SignatureHeaderName, signature);
        request.Headers.TryAddWithoutValidation(value.TimestampHeaderName, timestamp);
        request.Headers.TryAddWithoutValidation(value.KeyIdHeaderName, signingKey.KeyId);
        request.Headers.TryAddWithoutValidation(value.SourceHeaderName, value.Source);
        request.Headers.TryAddWithoutValidation(value.NonceHeaderName, nonce);
        request.Headers.TryAddWithoutValidation(value.ContentHashHeaderName, contentHash);
        request.Headers.TryAddWithoutValidation(RequestContextSupport.CorrelationIdHeaderName, correlationId);

        return await base.SendAsync(request, cancellationToken);
    }

    private string ResolveCorrelationId()
    {
        var context = httpContextAccessor.HttpContext;
        if (context is null)
        {
            return Guid.NewGuid().ToString("N");
        }

        return RequestContextSupport.GetOrCreateCorrelationId(context);
    }

    private static async Task<string> ComputeBodyHashAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content is null)
        {
            return EmptyBodyHash;
        }

        var body = await request.Content.ReadAsByteArrayAsync(cancellationToken);
        return body.Length == 0 ? EmptyBodyHash : Convert.ToHexString(SHA256.HashData(body));
    }

    private static async Task<string> BuildCanonicalRequestAsync(
        HttpRequestMessage request,
        string timestamp,
        string nonce,
        string correlationId,
        string source,
        string contentHash,
        CancellationToken cancellationToken)
    {
        var uri = request.RequestUri ?? throw new InvalidOperationException("Identity request URI is missing.");
        var normalizedPath = NormalizePath(uri.IsAbsoluteUri ? uri.AbsolutePath : uri.GetComponents(UriComponents.Path, UriFormat.Unescaped));
        var normalizedQuery = NormalizeQuery(uri);
        var normalizedContentType = request.Content?.Headers.ContentType?.ToString().Trim().ToLowerInvariant() ?? string.Empty;

        await Task.CompletedTask.WaitAsync(cancellationToken);

        return string.Join('\n',
            request.Method.Method.ToUpperInvariant(),
            normalizedPath,
            normalizedQuery,
            timestamp,
            nonce,
            correlationId,
            normalizedContentType,
            source,
            contentHash);
    }

    private static string NormalizePath(string? rawPath)
    {
        if (string.IsNullOrWhiteSpace(rawPath))
        {
            return "/";
        }

        var hasTrailingSlash = rawPath.Length > 1 && rawPath.EndsWith('/');
        var segments = rawPath
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.EscapeDataString);

        var normalized = "/" + string.Join('/', segments);
        if (hasTrailingSlash && !normalized.EndsWith('/'))
        {
            normalized += "/";
        }

        return normalized;
    }

    private static string NormalizeQuery(Uri uri)
    {
        var query = uri.IsAbsoluteUri ? uri.Query : uri.GetComponents(UriComponents.Query, UriFormat.UriEscaped);
        if (string.IsNullOrWhiteSpace(query))
        {
            return string.Empty;
        }

        query = query.TrimStart('?');
        var pairs = new List<(string Key, string Value)>();
        foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var index = part.IndexOf('=', StringComparison.Ordinal);
            var key = index >= 0 ? part[..index] : part;
            var value = index >= 0 ? part[(index + 1)..] : string.Empty;
            pairs.Add((Uri.UnescapeDataString(key), Uri.UnescapeDataString(value)));
        }

        return string.Join('&', pairs
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .ThenBy(pair => pair.Value, StringComparer.Ordinal)
            .Select(pair => Uri.EscapeDataString(pair.Key) + "=" + Uri.EscapeDataString(pair.Value)));
    }

    private static string ComputeSignature(string secret, string canonical)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(canonical)));
    }
}
