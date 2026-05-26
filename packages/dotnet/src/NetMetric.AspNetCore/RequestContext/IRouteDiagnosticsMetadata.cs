// <copyright file="IRouteDiagnosticsMetadata.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.AspNetCore.RequestContext;

public interface IRouteDiagnosticsMetadata
{
    string Route { get; }
}
