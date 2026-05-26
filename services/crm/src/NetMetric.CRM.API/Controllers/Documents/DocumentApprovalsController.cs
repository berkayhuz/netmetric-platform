// <copyright file="DocumentApprovalsController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.DocumentManagement.Application.Features.Approvals.Commands.SubmitDocumentReview;

namespace NetMetric.CRM.API.Controllers.Documents;

[ApiController]
[Route("api/documents/{documentId:guid}/reviews")]
[Authorize(Policy = AuthorizationPolicies.DocumentApprovalsManage)]
public sealed class DocumentApprovalsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<DocumentReviewSubmittedResponse>> Submit(
        Guid documentId,
        [FromBody] SubmitDocumentReviewRequest request,
        CancellationToken cancellationToken)
    {
        var reviewId = await mediator.Send(new SubmitDocumentReviewCommand(documentId, request.ReviewType), cancellationToken);
        return CreatedAtAction(nameof(Submit), new { documentId }, new DocumentReviewSubmittedResponse(reviewId));
    }

    public sealed record SubmitDocumentReviewRequest(string ReviewType);

    public sealed record DocumentReviewSubmittedResponse(Guid ReviewId);
}
