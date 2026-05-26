// <copyright file="AppendCustomerActivityCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerIntelligence.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerIntelligence.Contracts.DTOs;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.CustomerTimelineEntrys;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.CustomerIntelligence.Application.Features.Customer360.Commands.AppendCustomerActivity;

public sealed class AppendCustomerActivityCommandHandler(ICustomerIntelligenceDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<AppendCustomerActivityCommand, Customer360ActivityDto>
{
    public async Task<Customer360ActivityDto> Handle(AppendCustomerActivityCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();

        var entity = CustomerTimelineEntry.Create(request.Name, request.EntityType, request.RelatedEntityId, request.DataJson);
        entity.TenantId = currentUserService.TenantId;
        entity.SubjectType = request.SubjectType.Trim();
        entity.SubjectId = request.SubjectId;
        entity.Category = request.Category.Trim();
        entity.Channel = string.IsNullOrWhiteSpace(request.Channel) ? null : request.Channel.Trim();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.CreatedBy = currentUserService.UserName;
        entity.UpdatedBy = currentUserService.UserName;
        entity.OccurredAtUtc = request.OccurredAtUtc?.ToUniversalTime() ?? entity.OccurredAtUtc;

        await dbContext.CustomerTimelineEntrys.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }
}
