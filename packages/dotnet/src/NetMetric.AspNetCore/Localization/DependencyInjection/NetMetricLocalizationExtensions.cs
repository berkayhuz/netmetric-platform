// <copyright file="NetMetricLocalizationExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetMetric.AspNetCore.Localization.Providers;
using NetMetric.Localization;

namespace NetMetric.AspNetCore.Localization.DependencyInjection;

public static class NetMetricLocalizationExtensions
{
    public static IServiceCollection AddNetMetricLocalization(this IServiceCollection services)
    {
        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = NetMetricCultures.SupportedCultureNames
                .Select(culture => new CultureInfo(culture))
                .ToArray();

            options.DefaultRequestCulture = NetMetricCultures.ToRequestCulture(NetMetricCultures.DefaultCulture);
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
            options.FallBackToParentCultures = false;
            options.FallBackToParentUICultures = false;
            options.ApplyCurrentCultureToResponseHeaders = true;
            options.RequestCultureProviders =
            [
                new QueryStringRequestCultureProvider(),
                new NetMetricHeaderRequestCultureProvider(),
                new NetMetricCookieRequestCultureProvider(),
                new NetMetricAcceptLanguageRequestCultureProvider()
            ];
        });

        return services;
    }

    public static IApplicationBuilder UseNetMetricLocalization(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
        return app.UseRequestLocalization(options);
    }
}
