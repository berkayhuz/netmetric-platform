// <copyright file="DeleteNoteCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Activities;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Notes.Commands.DeleteNote;

public sealed class DeleteNoteCommandHandler(ICustomerManagementDbContext dbContext) : IRequestHandler<DeleteNoteCommand, Unit>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;

    public async Task<Unit> Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await _dbContext.Set<Note>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.NoteId, cancellationToken)
            ?? throw new NotFoundAppException("Note not found.");

        _dbContext.Set<Note>().Remove(note);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
