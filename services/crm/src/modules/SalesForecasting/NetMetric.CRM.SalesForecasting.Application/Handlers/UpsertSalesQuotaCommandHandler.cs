// <copyright file="UpsertSalesQuotaCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.SalesForecasting.Application.Abstractions.Persistence;
using NetMetric.CRM.SalesForecasting.Application.Commands;
using NetMetric.CRM.SalesForecasting.Contracts.DTOs;
using NetMetric.CRM.SalesForecasting.Domain.Entities;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.SalesForecasting.Application.Handlers;

public sealed class UpsertSalesQuotaCommandHandler(ISalesForecastingDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<UpsertSalesQuotaCommand, SalesQuotaDto>
{
    public async Task<SalesQuotaDto> Handle(UpsertSalesQuotaCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();

        var entity = await dbContext.SalesQuotas.FirstOrDefaultAsync(
            x => x.PeriodStart == request.PeriodStart
                 && x.PeriodEnd == request.PeriodEnd
                 && x.OwnerUserId == request.OwnerUserId,
            cancellationToken);

        if (entity is null)
        {
            entity = new SalesQuota
            {
                TenantId = currentUserService.TenantId,
                PeriodStart = request.PeriodStart,
                PeriodEnd = request.PeriodEnd,
                OwnerUserId = request.OwnerUserId,
                Amount = request.Amount,
                CurrencyCode = request.CurrencyCode.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = currentUserService.UserName,
                UpdatedBy = currentUserService.UserName
            };
            entity.SetNotes(request.Notes);
            await dbContext.SalesQuotas.AddAsync(entity, cancellationToken);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(request.RowVersion))
                entity.RowVersion = Convert.FromBase64String(request.RowVersion);

            entity.Amount = request.Amount;
            entity.CurrencyCode = request.CurrencyCode.Trim();
            entity.SetNotes(request.Notes);
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = currentUserService.UserName;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }
}
