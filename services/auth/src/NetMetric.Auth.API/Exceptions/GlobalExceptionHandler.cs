// <copyright file="GlobalExceptionHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetMetric.AspNetCore.ProblemDetails;
using NetMetric.Auth.Application.Exceptions;

namespace NetMetric.Auth.API.Exceptions;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is ValidationException validationException)
        {
            logger.LogWarning(
                "Auth request {Path} failed validation. Fields={Fields}",
                httpContext.Request.Path,
                string.Join(",", validationException.Errors.Select(error => error.PropertyName).Distinct()));

            var problemDetailsService = httpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
            var problemDetails = new ValidationProblemDetails(
                validationException.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray()))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = "One or more validation errors occurred.",
                Type = "https://httpstatuses.com/400",
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
            problemDetails.Extensions["correlationId"] = AspNetCore.RequestContext.RequestContextSupport.GetOrCreateCorrelationId(httpContext);
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails
            });

            return true;
        }

        if (exception is AuthApplicationException applicationException)
        {
            if (applicationException.StatusCode >= StatusCodes.Status500InternalServerError)
            {
                logger.LogError(
                    exception,
                    "Unhandled exception for auth request {Path}.",
                    httpContext.Request.Path);
            }
            else
            {
                logger.LogWarning(
                    "Auth request {Path} failed with {StatusCode}. ErrorCode={ErrorCode}",
                    httpContext.Request.Path,
                    applicationException.StatusCode,
                    applicationException.ErrorCode);
            }

            await ProblemDetailsSupport.WriteProblemAsync(
                httpContext,
                applicationException.StatusCode,
                applicationException.Title,
                applicationException.Message,
                type: applicationException.Type,
                errorCode: applicationException.ErrorCode,
                cancellationToken: cancellationToken);

            return true;
        }

        if (exception is DbUpdateConcurrencyException)
        {
            logger.LogWarning(
                exception,
                "Auth request {Path} encountered a concurrency conflict.",
                httpContext.Request.Path);

            await ProblemDetailsSupport.WriteProblemAsync(
                httpContext,
                StatusCodes.Status409Conflict,
                "Concurrency conflict",
                "The resource was modified by another request. Retry with the latest state.",
                errorCode: "concurrency_conflict",
                cancellationToken: cancellationToken);

            return true;
        }

        logger.LogError(exception, "Unhandled exception for auth request {Path}.", httpContext.Request.Path);

        await ProblemDetailsSupport.WriteProblemAsync(
            httpContext,
            StatusCodes.Status500InternalServerError,
            "Server error",
            "An unexpected error occurred.",
            errorCode: "server_error",
            cancellationToken: cancellationToken);

        return true;
    }
}
