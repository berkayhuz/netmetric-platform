// <copyright file="AuthRequestSizeLimits.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.API.Security;

public static class AuthRequestSizeLimits
{
    public const int AuthBodyBytes = 16 * 1024;
}
