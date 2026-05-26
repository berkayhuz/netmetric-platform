// <copyright file="NetMetricAcceptLanguageRequestCultureProvider.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using NetMetric.Localization;

namespace NetMetric.AspNetCore.Localization.Providers;

public sealed class NetMetricAcceptLanguageRequestCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        var header = httpContext.Request.Headers.AcceptLanguage.ToString();
        if (string.IsNullOrWhiteSpace(header))
        {
            return Task.FromResult<ProviderCultureResult?>(null);
        }

        foreach (var candidate in header.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var culture = NetMetricCultures.Normalize(candidate.Split(';')[0]);
            if (culture is not null)
            {
                return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(culture, culture));
            }
        }

        return Task.FromResult<ProviderCultureResult?>(null);
    }
}
