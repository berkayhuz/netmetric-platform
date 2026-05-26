// <copyright file="IAuthenticatorTotpService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Application.Abstractions;

public interface IAuthenticatorTotpService
{
    string GenerateSharedKey();
    string BuildAuthenticatorUri(string issuer, string accountName, string sharedKey);
    bool VerifyCode(string sharedKey, string verificationCode, DateTime utcNow);
}
