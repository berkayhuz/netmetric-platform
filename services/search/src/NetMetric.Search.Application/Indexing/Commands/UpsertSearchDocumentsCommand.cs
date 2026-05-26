// <copyright file="UpsertSearchDocumentsCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.Search.Application.Abstractions;
using NetMetric.Search.Contracts.Documents;

namespace NetMetric.Search.Application.Indexing.Commands;

public sealed record UpsertSearchDocumentsCommand(IReadOnlyCollection<SearchDocument> Documents) : IRequest;

public sealed class UpsertSearchDocumentsCommandHandler(ISearchIndexingService searchIndexingService)
    : IRequestHandler<UpsertSearchDocumentsCommand>
{
    public async Task Handle(UpsertSearchDocumentsCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Documents);
        if (request.Documents.Count == 0)
        {
            throw new ArgumentException("At least one search document is required.", nameof(request.Documents));
        }

        if (request.Documents.Any(document => document is null))
        {
            throw new ArgumentException("Search document collection cannot contain null documents.", nameof(request.Documents));
        }

        await searchIndexingService.UpsertManyAsync(request.Documents, cancellationToken);
    }
}

