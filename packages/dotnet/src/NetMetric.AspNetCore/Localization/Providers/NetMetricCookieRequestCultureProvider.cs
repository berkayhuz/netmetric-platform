// <copyright file="NetMetricCookieRequestCultureProvider.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using NetMetric.Localization;

namespace NetMetric.AspNetCore.Localization.Providers;

public sealed class NetMetricCookieRequestCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (!httpContext.Request.Cookies.TryGetValue(NetMetricCultures.CookieName, out var cookieValue))
        {
            return Task.FromResult<ProviderCultureResult?>(null);
        }

        var culture = NetMetricCultures.Normalize(cookieValue);
        if (culture is null)
        {
            var parsed = CookieRequestCultureProvider.ParseCookieValue(cookieValue);
            culture = NetMetricCultures.Normalize(parsed?.Cultures.FirstOrDefault().Value);
        }

        return Task.FromResult(culture is null ? null : new ProviderCultureResult(culture, culture));
    }
}
