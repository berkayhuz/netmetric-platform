// <copyright file="RowAccessLevel.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Authorization;

public enum RowAccessLevel
{
    None = 0,
    Own = 10,
    Assigned = 20,
    Tenant = 100
}
