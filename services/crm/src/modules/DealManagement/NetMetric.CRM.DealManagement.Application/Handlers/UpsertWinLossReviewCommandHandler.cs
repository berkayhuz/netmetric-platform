// <copyright file="UpsertWinLossReviewCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.DealManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.DealManagement.Application.Commands.Reviews;
using NetMetric.CRM.DealManagement.Application.Common;
using NetMetric.CRM.DealManagement.Contracts.DTOs;
using NetMetric.CRM.DealManagement.Domain.Entities;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.DealManagement.Application.Handlers;

public sealed class UpsertWinLossReviewCommandHandler(IDealManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<UpsertWinLossReviewCommand, WinLossReviewDto>
{
    public async Task<WinLossReviewDto> Handle(UpsertWinLossReviewCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        _ = await DealHandlerHelpers.RequireDealAsync(dbContext, request.DealId, cancellationToken);
        var entity = await dbContext.WinLossReviews.FirstOrDefaultAsync(x => x.DealId == request.DealId, cancellationToken);
        if (entity is null)
        {
            entity = new WinLossReview
            {
                TenantId = currentUserService.TenantId,
                DealId = request.DealId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserService.UserName
            };
            await dbContext.WinLossReviews.AddAsync(entity, cancellationToken);
        }
        else
        {
            var expected = DealHandlerHelpers.ParseRowVersion(request.RowVersion);
            if (expected.Length > 0)
                entity.RowVersion = expected;
        }

        entity.Outcome = request.Outcome.Trim();
        entity.Summary = string.IsNullOrWhiteSpace(request.Summary) ? null : request.Summary.Trim();
        entity.Strengths = string.IsNullOrWhiteSpace(request.Strengths) ? null : request.Strengths.Trim();
        entity.Risks = string.IsNullOrWhiteSpace(request.Risks) ? null : request.Risks.Trim();
        entity.CompetitorName = string.IsNullOrWhiteSpace(request.CompetitorName) ? null : request.CompetitorName.Trim();
        entity.CompetitorPrice = request.CompetitorPrice;
        entity.CustomerFeedback = string.IsNullOrWhiteSpace(request.CustomerFeedback) ? null : request.CustomerFeedback.Trim();
        entity.ReviewedAt = DateTime.UtcNow;
        entity.ReviewedByUserId = currentUserService.UserId;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUserService.UserName;

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }
}
