// <copyright file="TrustedInternalSourceAuthorizationHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using NetMetric.AspNetCore.TrustedGateway.Options;
using NetMetric.Auth.API.Middlewares;

namespace NetMetric.Auth.API.Security;

public sealed class TrustedInternalSourceAuthorizationHandler(
    IHttpContextAccessor httpContextAccessor,
    IOptions<InternalIdentityOptions> internalIdentityOptions,
    IOptions<TrustedGatewayOptions> trustedGatewayOptions) : AuthorizationHandler<TrustedInternalSourceRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TrustedInternalSourceRequirement requirement)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return Task.CompletedTask;
        }

        var trustedGateway = trustedGatewayOptions.Value;
        if (trustedGateway.RequireTrustedGateway && !TrustedGatewayMiddleware.IsTrustedGatewayRequest(httpContext))
        {
            return Task.CompletedTask;
        }

        var source = httpContext.Request.Headers[trustedGateway.SourceHeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(source))
        {
            return Task.CompletedTask;
        }

        if (!internalIdentityOptions.Value.AllowedSources.Contains(source, StringComparer.Ordinal))
        {
            return Task.CompletedTask;
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
