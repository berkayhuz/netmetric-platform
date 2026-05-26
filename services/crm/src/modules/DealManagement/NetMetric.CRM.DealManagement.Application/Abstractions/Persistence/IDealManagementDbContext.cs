// <copyright file="IDealManagementDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.DealManagement.Domain.Entities;
using NetMetric.CRM.Sales;
using NetMetric.Repository;

namespace NetMetric.CRM.DealManagement.Application.Abstractions.Persistence;

public interface IDealManagementDbContext : IUnitOfWork
{
    DbSet<Deal> Deals { get; }
    DbSet<LostReason> LostReasons { get; }
    DbSet<DealOutcomeHistory> DealOutcomeHistories { get; }
    DbSet<WinLossReview> WinLossReviews { get; }
    DbSet<GlobalTrashItem> GlobalTrashItems { get; }
}
