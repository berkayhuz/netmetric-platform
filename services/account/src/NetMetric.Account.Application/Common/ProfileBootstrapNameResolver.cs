// <copyright file="ProfileBootstrapNameResolver.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Account.Application.Abstractions.Identity;

namespace NetMetric.Account.Application.Common;

internal static class ProfileBootstrapNameResolver
{
    public static bool IsPlaceholderName(string firstName, string lastName)
        => string.Equals(firstName.Trim(), "New", StringComparison.OrdinalIgnoreCase) &&
           string.Equals(lastName.Trim(), "Member", StringComparison.OrdinalIgnoreCase);

    public static async Task<(string FirstName, string LastName)> ResolveAsync(
        IIdentityAccountClient identityAccountClient,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var members = await identityAccountClient.ListMembersAsync(tenantId, userId, cancellationToken);
            var currentMember = members.FirstOrDefault(x => x.UserId == userId);
            if (currentMember is not null)
            {
                var firstName = Normalize(currentMember.FirstName);
                var lastName = Normalize(currentMember.LastName);
                if (firstName is not null && lastName is not null)
                {
                    return (firstName, lastName);
                }

                var userNameFallback = Normalize(currentMember.UserName) ?? "New";
                return (firstName ?? userNameFallback, lastName ?? "Member");
            }
        }
        catch (IdentityServiceException)
        {
            // Fall back to deterministic defaults when identity service is unavailable.
        }

        return ("New", "Member");
    }

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        return normalized.Length == 0 ? null : normalized;
    }
}
