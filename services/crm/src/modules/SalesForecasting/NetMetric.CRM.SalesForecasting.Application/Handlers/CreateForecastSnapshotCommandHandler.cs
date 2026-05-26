// <copyright file="CreateForecastSnapshotCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.SalesForecasting.Application.Abstractions.Persistence;
using NetMetric.CRM.SalesForecasting.Application.Commands;
using NetMetric.CRM.SalesForecasting.Application.Queries;
using NetMetric.CRM.SalesForecasting.Contracts.DTOs;
using NetMetric.CRM.SalesForecasting.Domain.Entities;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.SalesForecasting.Application.Handlers;

public sealed class CreateForecastSnapshotCommandHandler(ISalesForecastingDbContext dbContext, ICurrentUserService currentUserService, IMediator mediator) : IRequestHandler<CreateForecastSnapshotCommand, ForecastSnapshotDto>
{
    public async Task<ForecastSnapshotDto> Handle(CreateForecastSnapshotCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();

        var summary = await mediator.Send(new GetSalesForecastSummaryQuery(request.PeriodStart, request.PeriodEnd, request.OwnerUserId), cancellationToken);

        var entity = new ForecastSnapshot
        {
            TenantId = currentUserService.TenantId,
            Name = request.Name.Trim(),
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            OwnerUserId = request.OwnerUserId,
            ForecastCategory = request.ForecastCategory.Trim(),
            PipelineAmount = summary.PipelineAmount,
            WeightedPipelineAmount = summary.WeightedPipelineAmount,
            BestCaseAmount = summary.BestCaseAmount,
            CommitAmount = summary.CommitAmount,
            ClosedWonAmount = summary.ClosedWonAmount,
            QuotaAmount = summary.QuotaAmount,
            AdjustmentAmount = summary.AdjustmentAmount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = currentUserService.UserName,
            UpdatedBy = currentUserService.UserName
        };
        entity.SetNotes(request.Notes);

        await dbContext.ForecastSnapshots.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }
}
