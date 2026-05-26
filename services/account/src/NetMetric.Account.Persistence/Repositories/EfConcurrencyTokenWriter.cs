// <copyright file="EfConcurrencyTokenWriter.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.Account.Application.Abstractions.Persistence;

namespace NetMetric.Account.Persistence.Repositories;

public sealed class EfConcurrencyTokenWriter(AccountDbContext dbContext) : IConcurrencyTokenWriter
{
    public void SetOriginalVersion<TEntity>(TEntity entity, byte[] version)
        where TEntity : class
        => dbContext.Entry(entity).Property("Version").OriginalValue = version;
}
