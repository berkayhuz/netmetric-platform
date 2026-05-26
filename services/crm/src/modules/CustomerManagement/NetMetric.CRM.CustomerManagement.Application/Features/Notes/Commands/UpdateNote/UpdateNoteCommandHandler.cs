// <copyright file="UpdateNoteCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Activities;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Common;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Notes;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Notes.Commands.UpdateNote;

public sealed class UpdateNoteCommandHandler(ICustomerManagementDbContext dbContext) : IRequestHandler<UpdateNoteCommand, NoteDto>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;

    public async Task<NoteDto> Handle(UpdateNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await _dbContext.Set<Note>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.NoteId, cancellationToken)
            ?? throw new NotFoundAppException("Note not found.");

        note.Title = request.Title.Trim();
        note.Content = request.Content.Trim();
        note.IsPinned = request.IsPinned;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new NoteDto
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.Content,
            IsPinned = note.IsPinned,
            EntityName = ResolveEntityName(note),
            EntityId = ResolveEntityId(note),
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt
        };
    }

    private static string ResolveEntityName(Note note)
        => note.CompanyId.HasValue ? EntityNames.Company
         : note.ContactId.HasValue ? EntityNames.Contact
         : EntityNames.Customer;

    private static Guid ResolveEntityId(Note note)
        => note.CompanyId ?? note.ContactId ?? note.CustomerId ?? Guid.Empty;
}
