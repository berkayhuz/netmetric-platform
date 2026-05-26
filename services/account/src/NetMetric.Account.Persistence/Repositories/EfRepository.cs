// <copyright file="EfRepository.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NetMetric.Account.Application.Abstractions.Persistence;

namespace NetMetric.Account.Persistence.Repositories;

public sealed class EfRepository<TContext, TEntity>(AccountDbContext dbContext) : IRepository<TContext, TEntity>
    where TContext : IAccountDbContext
    where TEntity : class
{
    public IQueryable<TEntity> Query => dbContext.Set<TEntity>();

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Set<TEntity>().FindAsync([id], cancellationToken);

    public Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => dbContext.Set<TEntity>().FirstOrDefaultAsync(predicate, cancellationToken);

    public Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        => ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await dbContext.Set<TEntity>().Where(predicate).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        => await ApplySpecification(specification).ToListAsync(cancellationToken);

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => dbContext.Set<TEntity>().AnyAsync(predicate, cancellationToken);

    public Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => dbContext.Set<TEntity>().CountAsync(predicate, cancellationToken);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);

    public void Update(TEntity entity) => dbContext.Set<TEntity>().Update(entity);

    public void Remove(TEntity entity) => dbContext.Set<TEntity>().Remove(entity);

    private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
        => specification.Criteria is null ? dbContext.Set<TEntity>() : dbContext.Set<TEntity>().Where(specification.Criteria);
}
