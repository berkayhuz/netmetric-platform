// <copyright file="DealHandlerHelpers.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.DealManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.DealManagement.Domain.Entities;
using NetMetric.CRM.Sales;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.DealManagement.Application.Handlers;

internal static class DealHandlerHelpers
{
    public static async Task<Deal> RequireDealAsync(IDealManagementDbContext dbContext, Guid dealId, CancellationToken cancellationToken)
        => await dbContext.Deals.FirstOrDefaultAsync(x => x.Id == dealId, cancellationToken) ?? throw new NotFoundAppException("Deal not found.");

    public static async Task<IReadOnlyList<DealOutcomeHistory>> LoadHistoryAsync(IDealManagementDbContext dbContext, Guid dealId, CancellationToken cancellationToken)
        => await dbContext.DealOutcomeHistories.Where(x => x.DealId == dealId).OrderByDescending(x => x.OccurredAt).ToListAsync(cancellationToken);

    public static async Task<WinLossReview?> LoadReviewAsync(IDealManagementDbContext dbContext, Guid dealId, CancellationToken cancellationToken)
        => await dbContext.WinLossReviews.FirstOrDefaultAsync(x => x.DealId == dealId, cancellationToken);

    public static byte[] ParseRowVersion(string? rowVersion) => string.IsNullOrWhiteSpace(rowVersion) ? [] : Convert.FromBase64String(rowVersion);

    public static void ApplyRowVersion(Deal entity, string? rowVersion)
    {
        var expected = ParseRowVersion(rowVersion);
        if (expected.Length > 0)
            entity.RowVersion = expected;
    }

    public static async Task AppendHistoryAsync(IDealManagementDbContext dbContext, ICurrentUserService currentUserService, Deal deal, string outcome, string stage, DateTime occurredAt, Guid? lostReasonId, string? note, CancellationToken cancellationToken)
    {
        await dbContext.DealOutcomeHistories.AddAsync(new DealOutcomeHistory
        {
            TenantId = currentUserService.TenantId,
            DealId = deal.Id,
            Outcome = outcome,
            Stage = stage,
            OccurredAt = occurredAt,
            ChangedByUserId = currentUserService.UserId,
            LostReasonId = lostReasonId,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = currentUserService.UserName,
            UpdatedBy = currentUserService.UserName
        }, cancellationToken);
    }
}
