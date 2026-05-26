// <copyright file="PublicCaptureSecurity.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.API.Security;

public interface IPublicCaptureChallengeVerifier
{
    Task<bool> VerifyAsync(string? captchaToken, HttpContext httpContext, CancellationToken cancellationToken);
}

public sealed class NoopPublicCaptureChallengeVerifier : IPublicCaptureChallengeVerifier
{
    public Task<bool> VerifyAsync(string? captchaToken, HttpContext httpContext, CancellationToken cancellationToken) =>
        Task.FromResult(true);
}
