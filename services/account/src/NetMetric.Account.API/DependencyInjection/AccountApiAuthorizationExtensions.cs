// <copyright file="AccountApiAuthorizationExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.Authorization;
using NetMetric.Account.Api.Authorization;
using NetMetric.Account.Application.Abstractions.Security;

namespace NetMetric.Account.Api.DependencyInjection;

public static class AccountApiAuthorizationExtensions
{
    public static IServiceCollection AddAccountApiAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        var authorizationOptions = configuration
            .GetSection(AccountAuthorizationOptions.SectionName)
            .Get<AccountAuthorizationOptions>() ?? new AccountAuthorizationOptions();

        services.AddSingleton<IAuthorizationHandler, AccountPermissionAuthorizationHandler>();
        services.AddAuthentication();
        services.AddAuthorizationBuilder()
            .AddAccountPolicy(AccountPolicies.AccountRead, authorizationOptions)
            .AddAccountPolicy(AccountPolicies.ConsentsReadOwn, authorizationOptions)
            .AddAccountPolicy(AccountPolicies.ConsentsAcceptOwn, authorizationOptions)
            .AddAccountPolicy(AccountPolicies.ProfileReadOwn, authorizationOptions)
            .AddAccountPolicy(AccountPolicies.ProfileUpdateOwn, authorizationOptions)
            .AddAccountPolicy(AccountPolicies.PreferencesReadOwn, authorizationOptions)
            .AddAccountPolicy(AccountPolicies.PreferencesUpdateOwn, authorizationOptions)
            .AddAccountPolicy(AccountPolicies.NotificationsReadOwn, authorizationOptions)
            .AddAccountPolicy(AccountPolicies.NotificationsUpdateOwn, authorizationOptions)
            .AddAccountPolicy(AccountPolicies.SessionsReadOwn, authorizationOptions)
            .AddAccountPolicy(AccountPolicies.SessionsRevokeOwn, authorizationOptions)
            .AddAccountPolicy(AccountPolicies.SecurityReadOwn, authorizationOptions)
            .AddAccountPolicy(AccountPolicies.SecurityChangePassword, authorizationOptions)
            .AddAccountPolicy(AccountPolicies.SecurityManageMfa, authorizationOptions)
            .AddAccountPolicy(AccountPolicies.DevicesReadOwn, authorizationOptions)
            .AddAccountPolicy(AccountPolicies.DevicesRevokeOwn, authorizationOptions)
            .AddAccountPolicy(AccountPolicies.UsersInvite, authorizationOptions)
            .AddAccountPolicy(AccountPolicies.UsersManage, authorizationOptions)
            .AddAccountPolicy(AccountPolicies.RolesRead, authorizationOptions)
            .AddAccountPolicy(AccountPolicies.RolesManage, authorizationOptions)
            .AddAccountPolicy(AccountPolicies.AuditRead, authorizationOptions);

        return services;
    }

    private static AuthorizationBuilder AddAccountPolicy(
        this AuthorizationBuilder builder,
        string policyName,
        AccountAuthorizationOptions options)
        => builder.AddPolicy(policyName, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.Requirements.Add(new AccountPermissionRequirement(policyName, options.RequiredPermissionClaimType));

            if (options.RequireTenantClaim)
            {
                policy.RequireAssertion(context =>
                    context.User.HasClaim(claim =>
                        claim.Type == options.RequiredTenantClaimType &&
                        Guid.TryParse(claim.Value, out var tenantId) &&
                        tenantId != Guid.Empty));
            }
        });
}
