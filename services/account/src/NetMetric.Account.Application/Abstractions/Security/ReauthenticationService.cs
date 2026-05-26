// <copyright file="ReauthenticationService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Account.Application.Common;
using NetMetric.Clock;

namespace NetMetric.Account.Application.Abstractions.Security;

public sealed class ReauthenticationService(IClock clock) : IReauthenticationService
{
    public Result EnsureSatisfied(CurrentUser currentUser, ReauthenticationRequirement requirement)
    {
        if (requirement.RequireSession && currentUser.SessionId is null)
        {
            return Result.Failure(Error.ReauthenticationRequired("An authenticated session is required for this operation."));
        }

        if (currentUser.AuthenticatedAt is null)
        {
            return Result.Failure(Error.ReauthenticationRequired());
        }

        if (clock.UtcNow - currentUser.AuthenticatedAt.Value > requirement.MaxAge)
        {
            return Result.Failure(Error.ReauthenticationRequired());
        }

        if (requirement.AcceptedAuthenticationMethods.Count > 0 &&
            !currentUser.AuthenticationMethods.Any(method => requirement.AcceptedAuthenticationMethods.Contains(method, StringComparer.OrdinalIgnoreCase)))
        {
            return Result.Failure(Error.ReauthenticationRequired("A stronger recent authentication method is required for this operation."));
        }

        return Result.Success();
    }
}
