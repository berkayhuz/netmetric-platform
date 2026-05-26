// <copyright file="AccountLifecycleOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Application.Options;

public sealed class AccountLifecycleOptions
{
    public const string SectionName = "AccountLifecycle";

    public bool RequireConfirmedEmailForLogin { get; set; } = true;
    public int EmailConfirmationTokenMinutes { get; set; } = 1440;
    public int PasswordResetTokenMinutes { get; set; } = 30;
    public int EmailChangeTokenMinutes { get; set; } = 30;
    public int InvitationTokenMinutes { get; set; } = 10080;
    public int TokenBytes { get; set; } = 32;
    public string PublicAppBaseUrl { get; set; } = "https://localhost:7025";
    public string ConfirmEmailPath { get; set; } = "/auth/confirm-email";
    public string ResetPasswordPath { get; set; } = "/auth/reset-password";
    public string ConfirmEmailChangePath { get; set; } = "/auth/confirm-email-change";
}
