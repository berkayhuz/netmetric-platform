// <copyright file="ToolsApiAuthorizationExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.Authorization;
using NetMetric.Tools.API.Options;

namespace NetMetric.Tools.API.DependencyInjection;

public static class ToolsApiAuthorizationExtensions
{
    public static IServiceCollection AddToolsApiAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(ToolsAuthorizationOptions.SectionName).Get<ToolsAuthorizationOptions>()
            ?? new ToolsAuthorizationOptions();

        services.AddAuthentication();

        services.AddAuthorizationBuilder()
            .AddPolicy(ToolsPolicies.ToolsHistoryReadOwn, policy => RequireHistoryAccess(policy, options))
            .AddPolicy(ToolsPolicies.ToolsHistoryWriteOwn, policy => RequireHistoryAccess(policy, options))
            .AddPolicy(ToolsPolicies.ToolsHistoryDeleteOwn, policy => RequireHistoryAccess(policy, options))
            .AddPolicy(ToolsPolicies.ToolsArtifactDownloadOwn, policy => RequireHistoryAccess(policy, options));

        return services;
    }

    private static void RequireHistoryAccess(AuthorizationPolicyBuilder policy, ToolsAuthorizationOptions options)
    {
        if (options.RequireAuthenticatedUserForHistory)
        {
            policy.RequireAuthenticatedUser();
        }
    }
}
