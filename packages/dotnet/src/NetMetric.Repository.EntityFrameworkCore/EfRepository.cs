// <copyright file="EfRepository.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace NetMetric.Repository.EntityFrameworkCore;

public sealed class EfRepository<TEntity, TKey>(DbContext dbContext) : IRepository<TEntity, TKey>
    where TEntity : class
{
    public IQueryable<TEntity> Query() => dbContext.Set<TEntity>();

    public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
        => await dbContext.Set<TEntity>().FindAsync([id], cancellationToken);

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => dbContext.Set<TEntity>().AnyAsync(predicate, cancellationToken);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);

    public void Update(TEntity entity) => dbContext.Set<TEntity>().Update(entity);

    public void Remove(TEntity entity) => dbContext.Set<TEntity>().Remove(entity);
}
