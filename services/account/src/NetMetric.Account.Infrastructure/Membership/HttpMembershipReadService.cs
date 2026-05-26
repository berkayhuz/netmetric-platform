// <copyright file="HttpMembershipReadService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMetric.Account.Application.Abstractions.Membership;
using NetMetric.Account.Contracts.Organizations;

namespace NetMetric.Account.Infrastructure.Membership;

public sealed class HttpMembershipReadService(
    HttpClient httpClient,
    IOptions<MembershipServiceOptions> options,
    ILogger<HttpMembershipReadService> logger) : IMembershipReadService
{
    public async Task<IReadOnlyCollection<OrganizationMembershipSummaryResponse>> GetMyOrganizationsAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (options.Value.SkipRemoteCalls)
        {
            return Array.Empty<OrganizationMembershipSummaryResponse>();
        }

        try
        {
            using var request = CreateRequest(
                HttpMethod.Get,
                options.Value.OrganizationsPath.Replace("{userId}", Uri.EscapeDataString(userId.ToString("D")), StringComparison.Ordinal),
                tenantId);

            using var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<IReadOnlyCollection<OrganizationMembershipSummaryResponse>>(cancellationToken: cancellationToken)
                ?? Array.Empty<OrganizationMembershipSummaryResponse>();
        }
        catch (Exception exception) when (IsFallbackAllowed(exception))
        {
            logger.LogWarning(
                exception,
                "Membership organization lookup failed for user {UserId} in tenant {TenantId}; returning an empty local-development fallback.",
                userId,
                tenantId);

            return Array.Empty<OrganizationMembershipSummaryResponse>();
        }
    }

    public async Task<PermissionOverviewResponse> GetMyPermissionsAsync(
        Guid tenantId,
        Guid userId,
        Guid? organizationId,
        CancellationToken cancellationToken = default)
    {
        if (options.Value.SkipRemoteCalls)
        {
            return new PermissionOverviewResponse(organizationId, DateTimeOffset.UtcNow, MayBeStale: true, Array.Empty<string>(), Array.Empty<PermissionGroupResponse>());
        }

        var path = options.Value.PermissionsPath.Replace("{userId}", Uri.EscapeDataString(userId.ToString("D")), StringComparison.Ordinal);
        if (organizationId is not null)
        {
            path = $"{path}?organizationId={Uri.EscapeDataString(organizationId.Value.ToString("D"))}";
        }

        try
        {
            using var request = CreateRequest(HttpMethod.Get, path, tenantId);
            using var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<PermissionOverviewResponse>(cancellationToken: cancellationToken)
                ?? new PermissionOverviewResponse(organizationId, DateTimeOffset.UtcNow, MayBeStale: true, Array.Empty<string>(), Array.Empty<PermissionGroupResponse>());
        }
        catch (Exception exception) when (IsFallbackAllowed(exception))
        {
            logger.LogWarning(
                exception,
                "Membership permission lookup failed for user {UserId} in tenant {TenantId}; returning a stale local-development fallback.",
                userId,
                tenantId);

            return new PermissionOverviewResponse(organizationId, DateTimeOffset.UtcNow, MayBeStale: true, Array.Empty<string>(), Array.Empty<PermissionGroupResponse>());
        }
    }

    private bool IsFallbackAllowed(Exception exception) =>
        options.Value.AllowUnavailableFallback &&
        exception is HttpRequestException or TaskCanceledException;

    private static HttpRequestMessage CreateRequest(HttpMethod method, string path, Guid tenantId)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.TryAddWithoutValidation("X-Tenant-Id", tenantId.ToString("D"));
        request.Headers.TryAddWithoutValidation("X-Gateway-Source", "NetMetric.Account");
        return request;
    }
}
