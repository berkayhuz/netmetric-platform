// <copyright file="WebhookOutboundRequestValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Globalization;
using System.Net;
using Microsoft.Extensions.Options;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Processing;

public sealed record ValidatedWebhookTarget(Uri Uri, string SafeTarget, IPAddress[] ResolvedAddresses);

public interface IWebhookDnsResolver
{
    Task<IPAddress[]> ResolveAsync(Uri uri, CancellationToken cancellationToken);
}

public sealed class SystemWebhookDnsResolver : IWebhookDnsResolver
{
    public async Task<IPAddress[]> ResolveAsync(Uri uri, CancellationToken cancellationToken)
    {
        if (IPAddress.TryParse(uri.Host, out var literal))
        {
            return [literal];
        }

        return await Dns.GetHostAddressesAsync(uri.IdnHost, cancellationToken);
    }
}

public sealed class WebhookOutboundRequestValidator(
    IOptions<WorkflowAutomationOptions> options,
    IWebhookDnsResolver dnsResolver)
{
    private static readonly IPAddress AwsMetadataAddress = IPAddress.Parse("169.254.169.254");
    private static readonly IPAddress AlibabaMetadataAddress = IPAddress.Parse("100.100.100.200");
    private static readonly IPAddress AzureMetadataAddress = IPAddress.Parse("168.63.129.16");
    private static readonly IPAddress GcpMetadataAddress = IPAddress.Parse("169.254.169.254");

    public async Task<ValidatedWebhookTarget> ValidateAsync(string targetUrl, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(targetUrl, UriKind.Absolute, out var uri))
        {
            throw new WorkflowPermanentException("Webhook target URL must be an absolute URI.", "webhook_target_invalid");
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new WorkflowPermanentException("Webhook target URL must use http or https.", "webhook_target_scheme_not_allowed");
        }

        var currentOptions = options.Value;
        if (uri.Scheme == Uri.UriSchemeHttp && !currentOptions.AllowHttpWebhookTargets)
        {
            throw new WorkflowPermanentException("Webhook target URL must use https.", "webhook_target_https_required");
        }

        if (!string.IsNullOrWhiteSpace(uri.UserInfo))
        {
            throw new WorkflowPermanentException("Webhook target URL must not contain user information.", "webhook_target_userinfo_not_allowed");
        }

        var host = NormalizeHost(uri);
        if (host is null)
        {
            throw new WorkflowPermanentException("Webhook target URL host is invalid.", "webhook_target_host_invalid");
        }

        if (currentOptions.WebhookAllowedHosts.Length > 0 &&
            !currentOptions.WebhookAllowedHosts.Any(pattern => HostMatches(pattern, host)))
        {
            throw new WorkflowPermanentException("Webhook target host is not allowed.", "webhook_target_host_not_allowed");
        }

        if (IPAddress.TryParse(uri.Host, out var literalAddress) && IsBlockedAddress(literalAddress))
        {
            throw new WorkflowPermanentException("Webhook target resolves to a blocked network address.", "webhook_target_network_not_allowed");
        }

        var addresses = await dnsResolver.ResolveAsync(uri, cancellationToken);
        if (addresses.Length == 0)
        {
            throw new WorkflowPermanentException("Webhook target host could not be resolved.", "webhook_target_dns_failed");
        }

        foreach (var address in addresses)
        {
            if (IsBlockedAddress(address))
            {
                throw new WorkflowPermanentException("Webhook target resolves to a blocked network address.", "webhook_target_network_not_allowed");
            }
        }

        return new ValidatedWebhookTarget(uri, CreateSafeAuditTarget(uri), addresses);
    }

    public static string CreateSafeAuditTarget(string targetUrl)
    {
        if (!Uri.TryCreate(targetUrl, UriKind.Absolute, out var uri))
        {
            return "invalid-target";
        }

        return CreateSafeAuditTarget(uri);
    }

    public static string CreateSafeAuditTarget(Uri uri)
    {
        var scheme = uri.Scheme is { Length: > 0 } ? uri.Scheme.ToLowerInvariant() : "unknown";
        var host = NormalizeHost(uri) ?? "invalid-host";
        var port = uri.IsDefaultPort ? string.Empty : $":{uri.Port}";
        return $"{scheme}://{host}{port}";
    }

    public static string SanitizeForAudit(Uri uri) => CreateSafeAuditTarget(uri);

    public static bool IsAllowedHostPattern(string? pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return false;
        }

        var value = pattern.Trim().TrimEnd('.').ToLowerInvariant();
        if (value is "*" or "." or "*.")
        {
            return false;
        }

        if (value.StartsWith("*."))
        {
            value = value[2..];
        }

        var ascii = ToAsciiHost(value);
        return ascii is not null && Uri.CheckHostName(ascii) == UriHostNameType.Dns && !IsLocalHostName(ascii);
    }

    private static string? NormalizeHost(Uri uri)
    {
        var host = ToAsciiHost(uri.IdnHost.Trim().TrimEnd('.').ToLowerInvariant());
        if (string.IsNullOrWhiteSpace(host) || IsLocalHostName(host))
        {
            return null;
        }

        return host;
    }

    private static string? ToAsciiHost(string host)
    {
        try
        {
            return new IdnMapping().GetAscii(host).ToLowerInvariant();
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static bool HostMatches(string pattern, string host)
    {
        var normalized = pattern.Trim().TrimEnd('.').ToLowerInvariant();
        if (normalized.StartsWith("*."))
        {
            var suffix = ToAsciiHost(normalized[2..]);
            if (suffix is null || host.Equals(suffix, StringComparison.Ordinal) || !host.EndsWith("." + suffix, StringComparison.Ordinal))
            {
                return false;
            }

            var prefix = host[..^(suffix.Length + 1)];
            return !prefix.Contains('.', StringComparison.Ordinal);
        }

        return ToAsciiHost(normalized) is { } exact && host.Equals(exact, StringComparison.Ordinal);
    }

    private static bool IsLocalHostName(string host) =>
        host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
        host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase) ||
        host.EndsWith(".local", StringComparison.OrdinalIgnoreCase) ||
        host.EndsWith(".internal", StringComparison.OrdinalIgnoreCase);

    private static bool IsBlockedAddress(IPAddress address)
    {
        if (IPAddress.IsLoopback(address) ||
            IPAddress.Any.Equals(address) ||
            IPAddress.IPv6Any.Equals(address) ||
            IPAddress.None.Equals(address) ||
            IPAddress.IPv6None.Equals(address) ||
            address.IsIPv6Multicast ||
            address.IsIPv6LinkLocal ||
            address.IsIPv6SiteLocal ||
            address.Equals(AwsMetadataAddress) ||
            address.Equals(AlibabaMetadataAddress) ||
            address.Equals(AzureMetadataAddress) ||
            address.Equals(GcpMetadataAddress))
        {
            return true;
        }

        if (address.IsIPv4MappedToIPv6)
        {
            return IsBlockedAddress(address.MapToIPv4());
        }

        var bytes = address.GetAddressBytes();
        return address.AddressFamily switch
        {
            System.Net.Sockets.AddressFamily.InterNetwork => IsBlockedIPv4(bytes),
            System.Net.Sockets.AddressFamily.InterNetworkV6 => (bytes[0] & 0xfe) == 0xfc,
            _ => true
        };
    }

    private static bool IsBlockedIPv4(byte[] bytes) =>
        bytes[0] == 0 ||
        bytes[0] == 10 ||
        bytes[0] == 127 ||
        bytes[0] >= 224 ||
        bytes[0] == 169 && bytes[1] == 254 ||
        bytes[0] == 172 && bytes[1] is >= 16 and <= 31 ||
        bytes[0] == 192 && bytes[1] == 168 ||
        bytes[0] == 100 && bytes[1] is >= 64 and <= 127;
}
