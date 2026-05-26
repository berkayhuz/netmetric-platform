// <copyright file="NullScope.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.AspNetCore.RequestContext;

internal sealed class NullScope : IDisposable
{
    public static NullScope Instance { get; } = new();

    public void Dispose()
    {
    }
}
