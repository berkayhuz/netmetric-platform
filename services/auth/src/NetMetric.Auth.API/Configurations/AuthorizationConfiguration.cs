// <copyright file="AuthorizationConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.Authorization;
using NetMetric.Auth.API.Permissions;
using NetMetric.Auth.API.Security;
using AuthorizationOptions = NetMetric.Auth.Application.Options.AuthorizationOptions;

namespace NetMetric.Auth.API.Configurations;

public static class AuthorizationConfiguration
{
    public static IServiceCollection AddNetMetricAuthorization(
        this IServiceCollection services,
        AuthorizationOptions options)
    {
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, TrustedInternalSourceAuthorizationHandler>();

        services.AddAuthorization(builder =>
        {
            builder.AddPolicy(AuthAuthorizationPolicies.TenantUser, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("tenant_id");
            });

            builder.AddPolicy(AuthAuthorizationPolicies.InternalService, policy =>
            {
                policy.Requirements.Add(new TrustedInternalSourceRequirement());
            });

            foreach (var definition in options.Policies)
            {
                builder.AddPolicy(definition.Name, policy =>
                {
                    if (definition.RequireAuthenticatedUser)
                    {
                        policy.RequireAuthenticatedUser();
                    }

                    if (definition.RequireTenantClaim)
                    {
                        policy.RequireClaim("tenant_id");
                    }

                    foreach (var permission in definition.RequiredPermissions)
                    {
                        policy.Requirements.Add(new PermissionRequirement(permission));
                    }

                    if (definition.RequiredRoles.Length > 0)
                    {
                        policy.RequireRole(definition.RequiredRoles);
                    }
                });
            }
        });

        return services;
    }
}
