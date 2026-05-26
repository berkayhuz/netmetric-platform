// <copyright file="SecuritySupport.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using IPNetwork = System.Net.IPNetwork;

namespace NetMetric.AspNetCore.Security;

public static class SecuritySupport
{
    public static void ApplySecurityHeaders(HttpContext context, SecurityHeadersValues values)
    {
        if (!context.Response.HasStarted)
        {
            Apply();
        }

        context.Response.OnStarting(() =>
        {
            Apply();
            return Task.CompletedTask;
        });

        void Apply()
        {
            var headers = context.Response.Headers;
            headers.XContentTypeOptions = "nosniff";
            headers.XFrameOptions = "DENY";
            headers["Referrer-Policy"] = values.ReferrerPolicy;
            headers.ContentSecurityPolicy = values.ContentSecurityPolicy;
            headers["Permissions-Policy"] = values.PermissionsPolicy;
            headers["X-Permitted-Cross-Domain-Policies"] = "none";
            headers["Cross-Origin-Opener-Policy"] = "same-origin";

            if (values.DisableResponseCaching)
            {
                headers.CacheControl = "no-store";
                headers.Pragma = "no-cache";
            }

            if (context.Request.IsHttps && values.EnableHsts)
            {
                var hstsValue = $"max-age={values.HstsMaxAgeSeconds}";
                if (values.IncludeSubDomains)
                {
                    hstsValue += "; includeSubDomains";
                }

                if (values.PreloadHsts)
                {
                    hstsValue += "; preload";
                }

                headers.StrictTransportSecurity = hstsValue;
            }
        }
    }

    public static void ConfigureForwardedHeaders(
        ForwardedHeadersOptions target,
        int forwardLimit,
        IEnumerable<string> knownProxies,
        IEnumerable<string> knownNetworks)
    {
        target.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor |
            ForwardedHeaders.XForwardedProto |
            ForwardedHeaders.XForwardedHost;
        target.ForwardLimit = forwardLimit;
        target.KnownProxies.Clear();
        target.KnownIPNetworks.Clear();

        foreach (var proxy in knownProxies)
        {
            if (IPAddress.TryParse(proxy, out var address))
            {
                target.KnownProxies.Add(address);
            }
        }

        foreach (var network in knownNetworks)
        {
            if (TryParseNetwork(network, out var parsedNetwork))
            {
                target.KnownIPNetworks.Add(parsedNetwork);
            }
        }
    }

    public static bool TryParseNetwork(string value, out IPNetwork network)
    {
        network = default;
        var segments = value.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length != 2 ||
            !IPAddress.TryParse(segments[0], out var prefix) ||
            !int.TryParse(segments[1], out var prefixLength))
        {
            return false;
        }

        network = new IPNetwork(prefix, prefixLength);
        return true;
    }

    public static bool IsValidOrigin(string value, bool allowHttpForLocalhostOnly)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(uri.AbsolutePath) && uri.AbsolutePath != "/")
        {
            return false;
        }

        if (!string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
        {
            return false;
        }

        if (uri.Scheme == Uri.UriSchemeHttps)
        {
            return true;
        }

        return allowHttpForLocalhostOnly &&
               uri.Scheme == Uri.UriSchemeHttp &&
               (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase));
    }

    public static bool LooksLikeHostName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return !value.Contains('/') &&
               !value.Contains("://", StringComparison.Ordinal) &&
               Uri.CheckHostName(value) != UriHostNameType.Unknown;
    }

    public static bool IsAllowedRemoteAddress(IPAddress? address, IEnumerable<string> exactAddresses, IEnumerable<string> networks)
    {
        if (address is null)
        {
            return false;
        }

        var proxyMatches = exactAddresses.Any(candidate => IPAddress.TryParse(candidate, out var parsed) && parsed.Equals(address));
        if (proxyMatches)
        {
            return true;
        }

        foreach (var network in networks)
        {
            if (TryParseNetwork(network, out var parsedNetwork) && parsedNetwork.Contains(address))
            {
                return true;
            }
        }

        return false;
    }
}
