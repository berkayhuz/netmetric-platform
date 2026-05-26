// <copyright file="CreateWorkTaskCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.WorkManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.WorkManagement.Contracts.DTOs;
using NetMetric.CRM.WorkManagement.Domain.Entities;

namespace NetMetric.CRM.WorkManagement.Application.Commands.Tasks.CreateWorkTask;

public sealed class CreateWorkTaskCommandHandler(IWorkManagementDbContext dbContext) : IRequestHandler<CreateWorkTaskCommand, WorkTaskDto>
{
    public async Task<WorkTaskDto> Handle(CreateWorkTaskCommand request, CancellationToken cancellationToken)
    {
        var entity = new WorkTask(request.Title, request.Description, request.OwnerUserId, request.DueAtUtc, request.Priority);
        await dbContext.Tasks.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new WorkTaskDto(
            entity.Id,
            entity.Title,
            entity.Description,
            entity.OwnerUserId,
            entity.DueAtUtc,
            entity.ReminderAtUtc,
            entity.Priority,
            entity.Status.ToString(),
            entity.CompletedAtUtc,
            entity.CompletedByUserId,
            entity.CompletionNote);
    }
}
