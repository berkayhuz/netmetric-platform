// <copyright file="CrmRequestContextMiddleware.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using NetMetric.AspNetCore.RequestContext;

namespace NetMetric.CRM.API.Middleware;

public sealed class CrmRequestContextMiddleware(
    RequestDelegate next,
    ILogger<CrmRequestContextMiddleware> logger)
{
    private static readonly Meter Meter = new("NetMetric.CRM.API");
    private static readonly Histogram<double> RequestDuration = Meter.CreateHistogram<double>(
        "crm_api_request_duration_ms",
        "ms",
        "Duration of CRM API requests.");

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        using var scope = RequestContextSupport.BeginScope(context, logger);
        context.Response.OnStarting(() =>
        {
            var duration = stopwatch.Elapsed.TotalMilliseconds.ToString("0.0", CultureInfo.InvariantCulture);
            context.Response.Headers.Append("Server-Timing", $"crm;dur={duration}");
            return Task.CompletedTask;
        });

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            RequestContextSupport.RecordCompletion(
                context,
                logger,
                RequestDuration,
                context.Request.Path,
                stopwatch.Elapsed.TotalMilliseconds,
                "crm-api");
        }
    }
}
