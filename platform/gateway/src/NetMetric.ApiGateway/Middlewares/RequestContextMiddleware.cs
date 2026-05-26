// <copyright file="RequestContextMiddleware.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using NetMetric.AspNetCore.RequestContext;

namespace NetMetric.ApiGateway.Middlewares;

public sealed class RequestContextMiddleware(RequestDelegate next, ILogger<RequestContextMiddleware> logger)
{
    private static readonly Histogram<double> RequestDuration = GatewayDiagnosticsMeter.Instance.CreateHistogram<double>(
        "gateway.request.duration",
        unit: "ms",
        description: "Gateway request latency");

    public async Task InvokeAsync(HttpContext context)
    {
        using var scope = RequestContextSupport.BeginScope(context, logger);
        var stopwatch = Stopwatch.StartNew();
        context.Response.OnStarting(() =>
        {
            var duration = stopwatch.Elapsed.TotalMilliseconds.ToString("0.0", CultureInfo.InvariantCulture);
            context.Response.Headers.Append("Server-Timing", $"gateway;dur={duration}");
            return Task.CompletedTask;
        });

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            RequestContextSupport.RecordCompletion(context, logger, RequestDuration, "proxy", stopwatch.Elapsed.TotalMilliseconds, "gateway");
        }
    }

    public static string GetOrCreateCorrelationId(HttpContext context) => RequestContextSupport.GetOrCreateCorrelationId(context);
}

internal static class GatewayDiagnosticsMeter
{
    public static readonly Meter Instance = new("NetMetric.ApiGateway.Requests");
}
