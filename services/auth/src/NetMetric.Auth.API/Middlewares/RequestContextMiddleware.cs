// <copyright file="RequestContextMiddleware.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using NetMetric.AspNetCore.RequestContext;

namespace NetMetric.Auth.API.Middlewares;

public sealed class RequestContextMiddleware(RequestDelegate next, ILogger<RequestContextMiddleware> logger)
{
    private static readonly Histogram<double> RequestDuration = AuthApiDiagnosticsMeter.Instance.CreateHistogram<double>(
        "auth.request.duration",
        unit: "ms",
        description: "Authentication API request latency");

    public async Task InvokeAsync(HttpContext context)
    {
        using var scope = RequestContextSupport.BeginScope(context, logger);
        var stopwatch = Stopwatch.StartNew();
        context.Response.OnStarting(() =>
        {
            var duration = stopwatch.Elapsed.TotalMilliseconds.ToString("0.0", CultureInfo.InvariantCulture);
            context.Response.Headers.Append("Server-Timing", $"auth;dur={duration}");
            return Task.CompletedTask;
        });

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            RequestContextSupport.RecordCompletion(context, logger, RequestDuration, "unmatched", stopwatch.Elapsed.TotalMilliseconds, "auth");
        }
    }

    public static string GetOrCreateCorrelationId(HttpContext context) => RequestContextSupport.GetOrCreateCorrelationId(context);
}

internal static class AuthApiDiagnosticsMeter
{
    public static readonly Meter Instance = new("NetMetric.Auth.API.Requests");
}
