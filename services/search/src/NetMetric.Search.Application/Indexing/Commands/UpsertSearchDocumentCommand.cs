// <copyright file="UpsertSearchDocumentCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.Search.Application.Abstractions;
using NetMetric.Search.Contracts.Documents;

namespace NetMetric.Search.Application.Indexing.Commands;

public sealed record UpsertSearchDocumentCommand(SearchDocument Document) : IRequest;

public sealed class UpsertSearchDocumentCommandHandler(ISearchIndexingService searchIndexingService)
    : IRequestHandler<UpsertSearchDocumentCommand>
{
    public async Task Handle(UpsertSearchDocumentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Document);

        await searchIndexingService.UpsertAsync(request.Document, cancellationToken);
    }
}

