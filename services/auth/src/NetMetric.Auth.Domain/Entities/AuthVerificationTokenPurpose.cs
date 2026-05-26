// <copyright file="AuthVerificationTokenPurpose.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Domain.Entities;

public static class AuthVerificationTokenPurpose
{
    public const string EmailConfirmation = "email-confirmation";
    public const string PasswordReset = "password-reset";
    public const string EmailChange = "email-change";
}
