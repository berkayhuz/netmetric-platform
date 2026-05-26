// <copyright file="DeleteNoteCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Notes.Commands.DeleteNote;

public sealed class DeleteNoteCommand : IRequest<Unit>
{
    public Guid NoteId { get; init; }
}
