// <copyright file="ArtificialIntelligenceController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.ArtificialIntelligence.Application.Commands.Providers.UpsertAiProvider;
using NetMetric.CRM.ArtificialIntelligence.Application.Queries.GetAiWorkspaceOverview;
using NetMetric.CRM.ArtificialIntelligence.Domain.Enums;

namespace NetMetric.CRM.API.Controllers.ArtificialIntelligence;

[ApiController]
[Route("api/artificial-intelligence")]
[Authorize(Policy = AuthorizationPolicies.ArtificialIntelligenceRead)]
public sealed class ArtificialIntelligenceController(IMediator mediator) : ControllerBase
{
    [HttpGet("workspace")]
    public async Task<IActionResult> GetWorkspace(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetAiWorkspaceOverviewQuery(), cancellationToken));

    [HttpPut("providers/{providerId:guid?}")]
    [Authorize(Policy = AuthorizationPolicies.ArtificialIntelligenceManage)]
    public async Task<IActionResult> UpsertProvider(
        Guid? providerId,
        [FromBody] UpsertAiProviderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpsertAiProviderCommand(
                providerId,
                request.Name,
                request.Provider,
                request.ModelName,
                request.Endpoint,
                request.SecretReference,
                request.Capabilities,
                request.IsActive),
            cancellationToken);

        return Ok(result);
    }

    public sealed record UpsertAiProviderRequest(
        string Name,
        [property: JsonRequired] AiProviderType Provider,
        string ModelName,
        string Endpoint,
        string SecretReference,
        IReadOnlyList<AiCapabilityType> Capabilities,
        [property: JsonRequired] bool IsActive);
}
