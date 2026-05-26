// <copyright file="GatewayExceptionHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.Diagnostics;
using NetMetric.AspNetCore.ProblemDetails;

namespace NetMetric.ApiGateway.Exceptions;

public sealed class GatewayExceptionHandler(ILogger<GatewayExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled gateway exception for {Path}", httpContext.Request.Path);

        await ProblemDetailsSupport.WriteProblemAsync(
            httpContext,
            StatusCodes.Status500InternalServerError,
            "Gateway request failed",
            "The gateway could not complete the request.",
            errorCode: "gateway_unhandled_exception",
            cancellationToken: cancellationToken);

        return true;
    }
}
