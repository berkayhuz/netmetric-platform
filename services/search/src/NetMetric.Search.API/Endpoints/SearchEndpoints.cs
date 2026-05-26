// <copyright file="SearchEndpoints.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetMetric.Search.API.Security;
using NetMetric.Search.Application.Abstractions;

namespace NetMetric.Search.API.Endpoints;

public static class SearchEndpoints
{
    public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/search", HandleSearchAsync)
            .AllowAnonymous();

        return endpoints;
    }

    public static async Task<IResult> HandleSearchAsync(
        HttpContext httpContext,
        ISearchAccessContextFactory accessContextFactory,
        ISearchQueryService searchQueryService,
        CancellationToken cancellationToken)
    {
        if (!SearchEndpointQueryParser.TryParse(httpContext.Request.Query, out var request, out var error))
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Invalid query parameters.",
                Detail = error,
                Status = StatusCodes.Status400BadRequest
            });
        }

        var accessContext = accessContextFactory.Create(httpContext.User);
        var response = await searchQueryService.SearchAsync(request, accessContext, cancellationToken);

        return Results.Ok(response);
    }
}
