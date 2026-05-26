// <copyright file="AccountAuditOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Infrastructure.Audit;

public sealed class AccountAuditOptions
{
    public const string SectionName = "Audit";

    public bool PersistAuditEntries { get; init; } = true;
    public int MetadataMaxLength { get; init; } = 4000;
    public string[] MaskedMetadataKeys { get; init; } =
    [
        "password",
        "token",
        "refreshToken",
        "accessToken",
        "authorization",
        "mfaSecret",
        "recoveryCode"
    ];
}
