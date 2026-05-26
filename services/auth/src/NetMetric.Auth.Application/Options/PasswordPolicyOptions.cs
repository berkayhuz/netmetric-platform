// <copyright file="PasswordPolicyOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Application.Options;

public sealed class PasswordPolicyOptions
{
    public const string SectionName = "PasswordPolicy";

    public int MinimumLength { get; set; } = 12;
    public int MaxLength { get; set; } = 128;
    public int RequiredUniqueChars { get; set; } = 4;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireNonAlphanumeric { get; set; } = true;
    public bool DisallowWhitespace { get; set; } = true;
    public List<string> BlacklistedPasswords { get; set; } = [];
}
