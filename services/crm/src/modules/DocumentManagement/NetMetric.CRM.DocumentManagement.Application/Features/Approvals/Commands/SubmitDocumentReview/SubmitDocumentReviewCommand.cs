// <copyright file="SubmitDocumentReviewCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.DocumentManagement.Contracts.DTOs;

namespace NetMetric.CRM.DocumentManagement.Application.Features.Approvals.Commands.SubmitDocumentReview;

public sealed record SubmitDocumentReviewCommand(Guid DocumentId, string ReviewType) : IRequest<Guid>;
