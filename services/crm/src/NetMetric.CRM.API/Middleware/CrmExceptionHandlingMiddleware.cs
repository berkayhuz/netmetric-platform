// <copyright file="CrmExceptionHandlingMiddleware.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.AspNetCore.ProblemDetails;
using NetMetric.Exceptions;

namespace NetMetric.CRM.API.Middleware;

public sealed class CrmExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<CrmExceptionHandlingMiddleware> logger,
    IWebHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            await WriteProblemAsync(context, exception);
        }
    }

    private async Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            logger.LogError(exception, "CRM request failed after the response started.");
            throw exception;
        }

        var descriptor = CreateProblemDescriptor(exception);
        if (descriptor.StatusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "CRM request failed with an unhandled exception.");
        }
        else
        {
            logger.LogWarning(exception, "CRM request failed with {StatusCode}.", descriptor.StatusCode);
        }

        context.Response.Clear();

        var detail = environment.IsDevelopment()
            ? exception.Message
            : descriptor.Detail;

        await ProblemDetailsSupport.WriteProblemAsync(
            context,
            descriptor.StatusCode,
            descriptor.Title,
            detail,
            errorCode: descriptor.ErrorCode,
            cancellationToken: context.RequestAborted);
    }

    private static ProblemDescriptor CreateProblemDescriptor(Exception exception)
        => exception switch
        {
            FluentValidation.ValidationException fluentValidationException => new(
                StatusCodes.Status400BadRequest,
                "Validation failed.",
                FormatValidationFailures(fluentValidationException),
                "validation_failed"),

            ValidationException appValidationException => new(
                StatusCodes.Status400BadRequest,
                "Validation failed.",
                appValidationException.Message,
                "validation_failed"),

            NotFoundException notFoundException => new(
                StatusCodes.Status404NotFound,
                "Resource not found.",
                notFoundException.Message,
                "not_found"),

            ConflictAppException conflictException => new(
                StatusCodes.Status409Conflict,
                "Conflict.",
                conflictException.Message,
                "conflict"),

            DbUpdateConcurrencyException concurrencyException => new(
                StatusCodes.Status409Conflict,
                "Concurrency conflict.",
                concurrencyException.Message,
                "concurrency_conflict"),

            ForbiddenAccessException forbiddenException => new(
                StatusCodes.Status403Forbidden,
                "Forbidden.",
                forbiddenException.Message,
                "forbidden"),

            UnauthorizedAccessException unauthorizedException => new(
                StatusCodes.Status401Unauthorized,
                "Authentication required.",
                unauthorizedException.Message,
                "unauthorized"),

            InvalidOperationException invalidOperationException
                when invalidOperationException.Message.Contains("tenant", StringComparison.OrdinalIgnoreCase) => new(
                    StatusCodes.Status403Forbidden,
                    "Tenant context rejected.",
                    invalidOperationException.Message,
                    "tenant_context_required"),

            _ => new(
                StatusCodes.Status500InternalServerError,
                "Unexpected server error.",
                "An unexpected error occurred while processing the CRM request.",
                "server_error")
        };

    private static string FormatValidationFailures(FluentValidation.ValidationException exception)
        => string.Join("; ", exception.Errors.Select(x => $"{x.PropertyName}: {x.ErrorMessage}"));

    private sealed record ProblemDescriptor(int StatusCode, string Title, string Detail, string ErrorCode);
}
