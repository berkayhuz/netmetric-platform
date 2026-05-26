// <copyright file="MediaSecurityScanRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Media.Models;

public sealed record MediaSecurityScanRequest(
    string OriginalFileName,
    string CanonicalContentType,
    Stream Content,
    long Length,
    string TenantId,
    string Purpose,
    string Module);
