// <copyright file="HttpIdentityAccountClient.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NetMetric.Account.Application.Abstractions.Identity;

namespace NetMetric.Account.Infrastructure.Identity;

public sealed class HttpIdentityAccountClient(HttpClient httpClient, IOptions<IdentityServiceOptions> options) : IIdentityAccountClient
{
    public async Task<AccountSecuritySummary> GetSecuritySummaryAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            options.Value.SecuritySummaryPath.Replace("{userId}", Uri.EscapeDataString(userId.ToString("D")), StringComparison.Ordinal));

        request.Headers.TryAddWithoutValidation("X-Tenant-Id", tenantId.ToString("D"));

        using var response = await SendCoreAsync(request, cancellationToken);

        return await response.Content.ReadFromJsonAsync<AccountSecuritySummary>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Identity service returned an empty security summary response.");
    }

    public Task<ChangePasswordIdentityResult> ChangePasswordAsync(
        Guid tenantId,
        Guid userId,
        ChangePasswordIdentityRequest request,
        CancellationToken cancellationToken = default)
        => SendJsonAsync<ChangePasswordIdentityRequest, ChangePasswordIdentityResult>(
            tenantId,
            userId,
            options.Value.ChangePasswordPath,
            request,
            cancellationToken);

    public Task<MfaStatusResult> GetMfaStatusAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
        => SendAsync<MfaStatusResult>(HttpMethod.Get, tenantId, userId, options.Value.MfaStatusPath, cancellationToken);

    public Task<MfaSetupResult> StartMfaSetupAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
        => SendAsync<MfaSetupResult>(HttpMethod.Post, tenantId, userId, options.Value.MfaSetupPath, cancellationToken);

    public Task<MfaConfirmResult> ConfirmMfaAsync(Guid tenantId, Guid userId, string verificationCode, CancellationToken cancellationToken = default)
        => SendJsonAsync<object, MfaConfirmResult>(
            tenantId,
            userId,
            options.Value.MfaConfirmPath,
            new { verificationCode },
            cancellationToken);

    public async Task DisableMfaAsync(Guid tenantId, Guid userId, string verificationCode, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Delete, tenantId, userId, options.Value.MfaDisablePath);
        request.Content = JsonContent.Create(new { verificationCode });
        using var response = await SendCoreAsync(request, cancellationToken);
    }

    public Task<RecoveryCodesResult> RegenerateRecoveryCodesAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
        => SendAsync<RecoveryCodesResult>(HttpMethod.Post, tenantId, userId, options.Value.RecoveryCodesRegeneratePath, cancellationToken);

    public Task<EmailChangeRequestIdentityResult> RequestEmailChangeAsync(
        Guid tenantId,
        Guid userId,
        EmailChangeRequestIdentityRequest request,
        CancellationToken cancellationToken = default)
        => SendJsonAsync<EmailChangeRequestIdentityRequest, EmailChangeRequestIdentityResult>(
            tenantId,
            userId,
            options.Value.EmailChangeRequestPath,
            request,
            cancellationToken);

    public Task<EmailChangeConfirmIdentityResult> ConfirmEmailChangeAsync(
        Guid tenantId,
        Guid userId,
        EmailChangeConfirmIdentityRequest request,
        CancellationToken cancellationToken = default)
        => SendJsonAsync<EmailChangeConfirmIdentityRequest, EmailChangeConfirmIdentityResult>(
            tenantId,
            userId,
            options.Value.EmailChangeConfirmPath,
            request,
            cancellationToken);

    public Task<TrustedDevicesIdentityResponse> GetTrustedDevicesAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
        => SendAsync<TrustedDevicesIdentityResponse>(HttpMethod.Get, tenantId, userId, options.Value.TrustedDevicesPath, cancellationToken);

    public async Task<bool> RevokeTrustedDeviceAsync(Guid tenantId, Guid userId, Guid deviceId, CancellationToken cancellationToken = default)
    {
        var path = options.Value.TrustedDeviceRevokePath
            .Replace("{deviceId}", Uri.EscapeDataString(deviceId.ToString("D")), StringComparison.Ordinal);
        using var request = CreateRequest(HttpMethod.Delete, tenantId, userId, path);
        using var response = await SendCoreAsync(request, cancellationToken, allowNotFound: true);
        return response.IsSuccessStatusCode;
    }

    public Task<TenantInvitationIdentityResponse> CreateInvitationAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateTenantInvitationIdentityRequest request,
        CancellationToken cancellationToken = default)
        => SendTenantJsonAsync<object, TenantInvitationIdentityResponse>(
            HttpMethod.Post,
            tenantId,
            options.Value.InvitationsPath,
            new
            {
                actorUserId,
                request.Email,
                request.FirstName,
                request.LastName
            },
            cancellationToken);

    public Task<IReadOnlyCollection<TenantInvitationSummaryIdentityResponse>> ListInvitationsAsync(
        Guid tenantId,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
        => SendTenantAsync<IReadOnlyCollection<TenantInvitationSummaryIdentityResponse>>(
            HttpMethod.Get,
            tenantId,
            AppendActor(options.Value.InvitationsPath, actorUserId),
            cancellationToken);

    public Task<TenantInvitationIdentityResponse> ResendInvitationAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid invitationId,
        CancellationToken cancellationToken = default)
        => SendTenantAsync<TenantInvitationIdentityResponse>(
            HttpMethod.Post,
            tenantId,
            AppendActor(ReplaceInvitationId(options.Value.InvitationResendPath, invitationId), actorUserId),
            cancellationToken);

    public Task<TenantInvitationIdentityResponse> RevokeInvitationAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid invitationId,
        CancellationToken cancellationToken = default)
        => SendTenantAsync<TenantInvitationIdentityResponse>(
            HttpMethod.Post,
            tenantId,
            AppendActor(ReplaceInvitationId(options.Value.InvitationRevokePath, invitationId), actorUserId),
            cancellationToken);

    public Task<IReadOnlyCollection<TenantMemberIdentityResponse>> ListMembersAsync(
        Guid tenantId,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
        => SendTenantAsync<IReadOnlyCollection<TenantMemberIdentityResponse>>(
            HttpMethod.Get,
            tenantId,
            AppendActor(options.Value.MembersPath, actorUserId),
            cancellationToken);

    public Task<IReadOnlyCollection<RoleCatalogIdentityResponse>> ListRoleCatalogAsync(
        Guid tenantId,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
        => SendTenantAsync<IReadOnlyCollection<RoleCatalogIdentityResponse>>(
            HttpMethod.Get,
            tenantId,
            AppendActor(options.Value.RolesCatalogPath, actorUserId),
            cancellationToken);

    public Task<TenantMemberIdentityResponse> UpdateMemberRolesAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid targetUserId,
        UpdateTenantMemberRolesIdentityRequest request,
        CancellationToken cancellationToken = default)
        => SendTenantJsonAsync<object, TenantMemberIdentityResponse>(
            HttpMethod.Put,
            tenantId,
            options.Value.MemberRolesPath.Replace("{userId}", Uri.EscapeDataString(targetUserId.ToString("D")), StringComparison.Ordinal),
            new
            {
                actorUserId,
                request.Roles
            },
            cancellationToken);

    private async Task<TResponse> SendAsync<TResponse>(
        HttpMethod method,
        Guid tenantId,
        Guid userId,
        string pathTemplate,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest(method, tenantId, userId, pathTemplate);
        using var response = await SendCoreAsync(request, cancellationToken);

        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Identity service returned an empty response.");
    }

    private async Task<TResponse> SendJsonAsync<TRequest, TResponse>(
        Guid tenantId,
        Guid userId,
        string pathTemplate,
        TRequest body,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Post, tenantId, userId, pathTemplate);
        request.Content = JsonContent.Create(body);
        using var response = await SendCoreAsync(request, cancellationToken);

        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Identity service returned an empty response.");
    }

    private async Task<TResponse> SendTenantAsync<TResponse>(
        HttpMethod method,
        Guid tenantId,
        string path,
        CancellationToken cancellationToken)
    {
        using var request = CreateTenantRequest(method, tenantId, path);
        using var response = await SendCoreAsync(request, cancellationToken);

        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Identity service returned an empty response.");
    }

    private async Task<TResponse> SendTenantJsonAsync<TRequest, TResponse>(
        HttpMethod method,
        Guid tenantId,
        string path,
        TRequest body,
        CancellationToken cancellationToken)
    {
        using var request = CreateTenantRequest(method, tenantId, path);
        request.Content = JsonContent.Create(body);
        using var response = await SendCoreAsync(request, cancellationToken);

        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Identity service returned an empty response.");
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, Guid tenantId, Guid userId, string pathTemplate)
    {
        var path = pathTemplate.Replace("{userId}", Uri.EscapeDataString(userId.ToString("D")), StringComparison.Ordinal);
        var request = new HttpRequestMessage(method, path);
        request.Headers.TryAddWithoutValidation("X-Tenant-Id", tenantId.ToString("D"));
        return request;
    }

    private static HttpRequestMessage CreateTenantRequest(HttpMethod method, Guid tenantId, string path)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.TryAddWithoutValidation("X-Tenant-Id", tenantId.ToString("D"));
        return request;
    }

    private static string ReplaceInvitationId(string path, Guid invitationId) =>
        path.Replace("{invitationId}", Uri.EscapeDataString(invitationId.ToString("D")), StringComparison.Ordinal);

    private static string AppendActor(string path, Guid actorUserId)
    {
        var separator = path.Contains('?', StringComparison.Ordinal) ? "&" : "?";
        return $"{path}{separator}actorUserId={Uri.EscapeDataString(actorUserId.ToString("D"))}";
    }

    private async Task<HttpResponseMessage> SendCoreAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken,
        bool allowNotFound = false)
    {
        try
        {
            var response = await httpClient.SendAsync(request, cancellationToken);
            if (allowNotFound && response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return response;
            }

            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var statusCode = (int)response.StatusCode;
            response.Dispose();
            throw new IdentityServiceException(
                "Identity service rejected the request.",
                $"identity_http_{statusCode}",
                statusCode,
                new HttpRequestException(body));
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new IdentityServiceException(
                "Identity service request timed out.",
                "identity_timeout",
                StatusCodes.Status504GatewayTimeout,
                exception);
        }
        catch (HttpRequestException exception)
        {
            throw new IdentityServiceException(
                "Identity service is unavailable.",
                "identity_unavailable",
                StatusCodes.Status503ServiceUnavailable,
                exception);
        }
    }
}
