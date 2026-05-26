// <copyright file="JwtOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Application.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenHours { get; set; } = 8;
    public int RefreshTokenPersistentDays { get; set; } = 14;
    public JwtSigningKeyOptions[] SigningKeys { get; set; } = [];
}

public sealed class JwtSigningKeyOptions
{
    public string KeyId { get; set; } = null!;
    public bool IsCurrent { get; set; }
    public string? PrivateKeyPath { get; set; }
    public string PrivateKeyPem { get; set; } = null!;
}
