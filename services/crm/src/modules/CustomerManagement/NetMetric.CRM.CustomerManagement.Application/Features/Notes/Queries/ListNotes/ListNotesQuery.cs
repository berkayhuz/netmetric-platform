// <copyright file="ListNotesQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Notes;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Notes.Queries.ListNotes;

public sealed class ListNotesQuery : IRequest<IReadOnlyList<NoteDto>>
{
    public required string EntityName { get; init; }
    public required Guid EntityId { get; init; }
}
