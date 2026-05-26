// <copyright file="AccountPolicies.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Application.Abstractions.Security;

public static class AccountPolicies
{
    public const string AccountRead = "account.read";
    public const string ConsentsReadOwn = "account.consents.read_own";
    public const string ConsentsAcceptOwn = "account.consents.accept_own";
    public const string ProfileReadOwn = "account.profile.read_own";
    public const string ProfileUpdateOwn = "account.profile.update_own";
    public const string PreferencesReadOwn = "account.preferences.read_own";
    public const string PreferencesUpdateOwn = "account.preferences.update_own";
    public const string NotificationsReadOwn = "account.notifications.read_own";
    public const string NotificationsUpdateOwn = "account.notifications.update_own";
    public const string SessionsReadOwn = "account.sessions.read_own";
    public const string SessionsRevokeOwn = "account.sessions.revoke_own";
    public const string SecurityReadOwn = "account.security.read_own";
    public const string SecurityChangePassword = "account.security.change_password";
    public const string SecurityManageMfa = "account.security.manage_mfa";
    public const string DevicesReadOwn = "account.devices.read_own";
    public const string DevicesRevokeOwn = "account.devices.revoke_own";
    public const string UsersInvite = "users.invite";
    public const string UsersManage = "users.manage";
    public const string RolesRead = "roles.read";
    public const string RolesManage = "roles.manage";
    public const string AuditRead = "audit.read";
}
