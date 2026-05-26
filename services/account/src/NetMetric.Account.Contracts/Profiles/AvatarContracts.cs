// <copyright file="AvatarContracts.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Contracts.Profiles;

public sealed record AvatarUploadResponse(
    Guid AssetId,
    string PublicUrl,
    string ContentType,
    long SizeBytes,
    int? Width,
    int? Height,
    string Status,
    string Purpose,
    DateTimeOffset CreatedAtUtc);
