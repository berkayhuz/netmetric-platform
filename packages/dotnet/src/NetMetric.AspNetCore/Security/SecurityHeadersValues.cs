// <copyright file="SecurityHeadersValues.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.AspNetCore.Security;

public sealed record SecurityHeadersValues(
    string ContentSecurityPolicy,
    string ReferrerPolicy,
    string PermissionsPolicy,
    bool EnableHsts,
    int HstsMaxAgeSeconds,
    bool PreloadHsts,
    bool IncludeSubDomains,
    bool DisableResponseCaching);
