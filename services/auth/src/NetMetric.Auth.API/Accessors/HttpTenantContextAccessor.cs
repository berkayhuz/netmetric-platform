// <copyright file="HttpTenantContextAccessor.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Auth.API.Middlewares;
using NetMetric.Auth.Application.Abstractions;
using NetMetric.Auth.Application.Records;

namespace NetMetric.Auth.API.Accessors;

public sealed class HttpTenantContextAccessor(IHttpContextAccessor httpContextAccessor) : ITenantContextAccessor
{
    public TenantContext? Current => httpContextAccessor.HttpContext is null
        ? null
        : TenantResolutionMiddleware.GetTenantContext(httpContextAccessor.HttpContext);
}
