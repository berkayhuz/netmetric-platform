// <copyright file="ResultHttpExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetMetric.Account.Application.Abstractions.Identity;
using NetMetric.Account.Application.Common;
using NetMetric.AspNetCore.RequestContext;

namespace NetMetric.Account.Api.Http;

public static class ResultHttpExtensions
{
    public static ActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return new NoContentResult();
        }

        return ToProblem(result.Error);
    }

    public static ActionResult<T> ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess && result.Value is not null)
        {
            return new OkObjectResult(result.Value);
        }

        return ToProblem(result.Error);
    }

    public static IApplicationBuilder UseAccountExceptionHandling(this IApplicationBuilder app)
        => app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
                var exception = feature?.Error;

                var problem = exception switch
                {
                    ValidationException validationException => new ValidationProblemDetails(
                        validationException.Errors
                            .GroupBy(error => error.PropertyName)
                            .ToDictionary(
                                group => group.Key,
                                group => group.Select(error => error.ErrorMessage).ToArray()))
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Validation failed.",
                        Type = "https://httpstatuses.com/400"
                    },
                    DbUpdateConcurrencyException => new ProblemDetails
                    {
                        Status = StatusCodes.Status409Conflict,
                        Title = "Concurrency conflict.",
                        Detail = "The resource was modified by another operation.",
                        Type = "https://httpstatuses.com/409"
                    },
                    IdentityServiceException identityException => new ProblemDetails
                    {
                        Status = identityException.StatusCode,
                        Title = "Identity service unavailable.",
                        Detail = "The requested security operation could not be completed right now.",
                        Type = $"https://httpstatuses.com/{identityException.StatusCode}",
                        Extensions =
                        {
                            ["code"] = identityException.ErrorCode
                        }
                    },
                    _ => new ProblemDetails
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "Unexpected error.",
                        Detail = "The operation could not be completed.",
                        Type = "https://httpstatuses.com/500"
                    }
                };

                problem.Extensions["traceId"] = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
                if (context.Items.TryGetValue(RequestContextSupport.CorrelationIdHeaderName, out var correlationId) &&
                    correlationId is string correlationIdValue &&
                    !string.IsNullOrWhiteSpace(correlationIdValue))
                {
                    problem.Extensions["correlationId"] = correlationIdValue;
                }
                else if (context.Request.Headers.TryGetValue(RequestContextSupport.CorrelationIdHeaderName, out var correlationIdHeader) &&
                         !string.IsNullOrWhiteSpace(correlationIdHeader))
                {
                    problem.Extensions["correlationId"] = correlationIdHeader.ToString();
                }

                context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(problem, context.RequestAborted);
            });
        });

    private static ObjectResult ToProblem(Error error)
    {
        var status = error.Code switch
        {
            "not_found" => StatusCodes.Status404NotFound,
            "forbidden" => StatusCodes.Status403Forbidden,
            "conflict" => StatusCodes.Status409Conflict,
            "reauth_required" => StatusCodes.Status401Unauthorized,
            "validation_error" => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        };

        return new ObjectResult(new ProblemDetails
        {
            Status = status,
            Title = error.Code,
            Detail = error.Message,
            Type = $"https://httpstatuses.com/{status}"
        }.WithErrorCode(error.Code))
        {
            StatusCode = status
        };
    }

    private static ProblemDetails WithErrorCode(this ProblemDetails problemDetails, string code)
    {
        problemDetails.Extensions["code"] = code;
        return problemDetails;
    }
}
