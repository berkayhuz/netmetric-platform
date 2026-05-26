// <copyright file="CreateNoteCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.Activities;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Common;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Notes;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Notes.Commands.CreateNote;

public sealed class CreateNoteCommandHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentUserService currentUserService) : IRequestHandler<CreateNoteCommand, NoteDto>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<NoteDto> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
    {
        _currentUserService.EnsureAuthenticated();

        var note = new Note
        {
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            IsPinned = request.IsPinned
        };

        CustomerManagementEntityReferenceHelper.ApplyReference(note, request.EntityName, request.EntityId);

        await _dbContext.Set<Note>().AddAsync(note, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new NoteDto
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.Content,
            IsPinned = note.IsPinned,
            EntityName = request.EntityName.ToLowerInvariant(),
            EntityId = request.EntityId,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt
        };
    }
}
