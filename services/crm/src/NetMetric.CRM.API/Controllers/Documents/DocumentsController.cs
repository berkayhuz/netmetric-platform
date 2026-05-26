// <copyright file="DocumentsController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.DocumentManagement.Application.Features.Documents.Commands.CreateDocument;
using NetMetric.CRM.DocumentManagement.Application.Features.Documents.Queries.GetDocumentMetadata;
using NetMetric.CRM.DocumentManagement.Application.Features.Documents.Queries.ListDocuments;
using NetMetric.CRM.DocumentManagement.Contracts.DTOs;
using NetMetric.Pagination;

namespace NetMetric.CRM.API.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize(Policy = AuthorizationPolicies.DocumentsRead)]
public sealed class DocumentsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<DocumentMetadataDto>>> Get(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListDocumentsQuery(page, pageSize, search), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{documentId:guid}")]
    public async Task<ActionResult<DocumentMetadataDto>> GetById(Guid documentId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDocumentMetadataQuery(documentId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.DocumentsManage)]
    public async Task<IActionResult> Create([FromBody] CreateDocumentRequest request, CancellationToken cancellationToken)
    {
        var documentId = await mediator.Send(
            new CreateDocumentCommand(request.Name, request.ContentType, request.SizeBytes),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { documentId }, new { documentId });
    }
}

public sealed record CreateDocumentRequest(string Name, string ContentType, [property: JsonRequired] long SizeBytes);
