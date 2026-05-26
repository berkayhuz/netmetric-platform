// <copyright file="ConcurrencyHelper.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.Entities.Abstractions;

namespace NetMetric.CRM.TicketManagement.Infrastructure.Services;

internal static class ConcurrencyHelper
{
    public static void ApplyRowVersion<TEntity>(DbContext dbContext, TEntity entity, byte[]? rowVersion)
        where TEntity : class, IHasRowVersion
    {
        if (rowVersion is null || rowVersion.Length == 0)
            return;

        dbContext.Entry(entity).Property(x => x.RowVersion).OriginalValue = rowVersion;
    }
}
