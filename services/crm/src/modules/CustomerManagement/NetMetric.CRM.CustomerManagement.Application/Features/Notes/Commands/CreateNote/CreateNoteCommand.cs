// <copyright file="CreateNoteCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Notes;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Notes.Commands.CreateNote;

public sealed class CreateNoteCommand : IRequest<NoteDto>
{
    public required string EntityName { get; init; }
    public required Guid EntityId { get; init; }
    public required string Title { get; init; }
    public required string Content { get; init; }
    public bool IsPinned { get; init; }
}
