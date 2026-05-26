// <copyright file="NetMetricHeaderRequestCultureProvider.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using NetMetric.Localization;

namespace NetMetric.AspNetCore.Localization.Providers;

public sealed class NetMetricHeaderRequestCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        var culture = httpContext.Request.Headers.TryGetValue(NetMetricCultures.HeaderName, out var value)
            ? NetMetricCultures.Normalize(value.FirstOrDefault())
            : null;

        return Task.FromResult(culture is null ? null : new ProviderCultureResult(culture, culture));
    }
}
