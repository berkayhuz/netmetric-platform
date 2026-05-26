// <copyright file="DetachDocumentReferenceCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Documents.Commands.DetachDocumentReference;

public sealed class DetachDocumentReferenceCommand : IRequest<Unit>
{
    public Guid DocumentId { get; init; }
}
