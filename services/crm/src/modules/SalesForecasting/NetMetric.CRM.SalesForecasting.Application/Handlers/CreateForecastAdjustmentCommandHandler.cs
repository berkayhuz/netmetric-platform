// <copyright file="CreateForecastAdjustmentCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.SalesForecasting.Application.Abstractions.Persistence;
using NetMetric.CRM.SalesForecasting.Application.Commands;
using NetMetric.CRM.SalesForecasting.Contracts.DTOs;
using NetMetric.CRM.SalesForecasting.Domain.Entities;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.SalesForecasting.Application.Handlers;

public sealed class CreateForecastAdjustmentCommandHandler(ISalesForecastingDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<CreateForecastAdjustmentCommand, ForecastAdjustmentDto>
{
    public async Task<ForecastAdjustmentDto> Handle(CreateForecastAdjustmentCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();

        var entity = new ForecastAdjustment
        {
            TenantId = currentUserService.TenantId,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            OwnerUserId = request.OwnerUserId,
            Amount = request.Amount,
            Reason = request.Reason.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = currentUserService.UserName,
            UpdatedBy = currentUserService.UserName
        };
        entity.SetNotes(request.Notes);

        await dbContext.ForecastAdjustments.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }
}
