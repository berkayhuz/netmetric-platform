// <copyright file="SoftDeleteSearchDocumentCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.Search.Application.Abstractions;

namespace NetMetric.Search.Application.Indexing.Commands;

public sealed record SoftDeleteSearchDocumentCommand(string DocumentId) : IRequest;

public sealed class SoftDeleteSearchDocumentCommandHandler(ISearchIndexingService searchIndexingService)
    : IRequestHandler<SoftDeleteSearchDocumentCommand>
{
    public async Task Handle(SoftDeleteSearchDocumentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.DocumentId))
        {
            throw new ArgumentException("Document id is required.", nameof(request.DocumentId));
        }

        await searchIndexingService.SoftDeleteAsync(request.DocumentId, cancellationToken);
    }
}

