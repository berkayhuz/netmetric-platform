// <copyright file="ISpecification.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Linq.Expressions;

namespace NetMetric.Repository;

public interface ISpecification<TEntity>
{
    Expression<Func<TEntity, bool>>? Criteria { get; }
}
