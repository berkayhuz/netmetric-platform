// <copyright file="RepositoryContracts.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Linq.Expressions;
namespace NetMetric.Account.Application.Abstractions.Persistence;

public interface IAccountDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ISpecification<TEntity>
    where TEntity : class
{
    Expression<Func<TEntity, bool>>? Criteria { get; }
}

public interface IRepository<TContext, TEntity>
    where TEntity : class
{
    IQueryable<TEntity> Query { get; }
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}

public interface IConcurrencyTokenWriter
{
    void SetOriginalVersion<TEntity>(TEntity entity, byte[] version)
        where TEntity : class;
}
