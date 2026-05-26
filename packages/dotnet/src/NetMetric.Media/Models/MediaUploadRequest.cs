// <copyright file="MediaUploadRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Media.Models;

public sealed record MediaUploadRequest(
    string TenantId,
    string Purpose,
    string? OwnerUserId,
    string OriginalFileName,
    string ContentType,
    Stream Content,
    long Length,
    string Module);
