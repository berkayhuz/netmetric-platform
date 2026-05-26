// <copyright file="ReauthenticationContracts.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Account.Application.Common;

namespace NetMetric.Account.Application.Abstractions.Security;

public sealed record ReauthenticationRequirement(
    string Operation,
    TimeSpan MaxAge,
    bool RequireSession,
    IReadOnlyCollection<string> AcceptedAuthenticationMethods);

public static class ReauthenticationOperations
{
    public const string ChangePassword = "change_password";
    public const string ManageMfa = "manage_mfa";
    public const string RegenerateRecoveryCodes = "regenerate_recovery_codes";
    public const string RevokeOtherSessions = "revoke_other_sessions";
    public const string RevokeTrustedDevice = "revoke_trusted_device";
    public const string RequestEmailChange = "request_email_change";
}

public interface IReauthenticationService
{
    Result EnsureSatisfied(CurrentUser currentUser, ReauthenticationRequirement requirement);
}
