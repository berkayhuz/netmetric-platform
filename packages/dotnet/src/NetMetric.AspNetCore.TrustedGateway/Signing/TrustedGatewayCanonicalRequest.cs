// <copyright file="TrustedGatewayCanonicalRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace NetMetric.AspNetCore.TrustedGateway.Signing;

public static class TrustedGatewayCanonicalRequest
{
    private static readonly string EmptyBodyHash = Convert.ToHexString(SHA256.HashData(Array.Empty<byte>()));

    public static async Task<string> BuildAsync(
        HttpRequest request,
        string timestamp,
        string nonce,
        string correlationId,
        string source,
        string contentHash,
        CancellationToken cancellationToken)
    {
        var normalizedPath = NormalizePath(request.Path.Value);
        var normalizedQuery = NormalizeQuery(request.Query);
        var normalizedContentType = NormalizeContentType(request.ContentType);

        await EnsureBufferedAsync(request, cancellationToken);

        return string.Join('\n',
            request.Method.ToUpperInvariant(),
            normalizedPath,
            normalizedQuery,
            timestamp,
            nonce,
            correlationId,
            normalizedContentType,
            source,
            contentHash);
    }

    public static async Task<string> ComputeBodyHashAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        if (request.Body is null)
        {
            return EmptyBodyHash;
        }

        await EnsureBufferedAsync(request, cancellationToken);
        request.Body.Position = 0;

        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(request.Body, cancellationToken);
        request.Body.Position = 0;
        return hash.Length == 0 ? EmptyBodyHash : Convert.ToHexString(hash);
    }

    private static async Task EnsureBufferedAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        request.EnableBuffering();

        if (!request.Body.CanSeek)
        {
            await request.Body.CopyToAsync(Stream.Null, cancellationToken);
        }
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

    private static string NormalizeQuery(IQueryCollection query)
    {
        if (query.Count == 0)
        {
            return string.Empty;
        }

        var pairs = new List<string>();

        foreach (var key in query.Keys.OrderBy(x => x, StringComparer.Ordinal))
        {
            var encodedKey = Uri.EscapeDataString(key);
            var values = query[key];
            if (StringValues.IsNullOrEmpty(values))
            {
                pairs.Add(encodedKey + "=");
                continue;
            }

            foreach (var value in values.OrderBy(x => x, StringComparer.Ordinal))
            {
                pairs.Add(encodedKey + "=" + Uri.EscapeDataString(value ?? string.Empty));
            }
        }

        return string.Join('&', pairs);
    }

    private static string NormalizeContentType(string? contentType) =>
        string.IsNullOrWhiteSpace(contentType)
            ? string.Empty
            : contentType.Trim().ToLowerInvariant();
}
