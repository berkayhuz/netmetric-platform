// <copyright file="DocumentVersionsController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.DocumentManagement.Application.Features.Versions.Commands.AddDocumentVersion;

namespace NetMetric.CRM.API.Controllers.Documents;

[ApiController]
[Route("api/documents/{documentId:guid}/versions")]
[Authorize(Policy = AuthorizationPolicies.DocumentVersionsManage)]
public sealed class DocumentVersionsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<DocumentVersionCreatedResponse>> Create(
        Guid documentId,
        [FromBody] AddDocumentVersionRequest request,
        CancellationToken cancellationToken)
    {
        var versionId = await mediator.Send(
            new AddDocumentVersionCommand(documentId, request.FileName, request.StorageKey),
            cancellationToken);

        return CreatedAtAction(nameof(Create), new { documentId }, new DocumentVersionCreatedResponse(versionId));
    }

    public sealed record AddDocumentVersionRequest(string FileName, string StorageKey);

    public sealed record DocumentVersionCreatedResponse(Guid VersionId);
}
