// <copyright file="SetNotePinnedCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Activities;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Notes.Commands.SetNotePinned;

public sealed class SetNotePinnedCommandHandler(ICustomerManagementDbContext dbContext) : IRequestHandler<SetNotePinnedCommand, Unit>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;

    public async Task<Unit> Handle(SetNotePinnedCommand request, CancellationToken cancellationToken)
    {
        var note = await _dbContext.Set<Note>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.NoteId, cancellationToken)
            ?? throw new NotFoundAppException("Note not found.");

        note.IsPinned = request.IsPinned;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
