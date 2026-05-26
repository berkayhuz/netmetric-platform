// <copyright file="SearchDocumentVisibility.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Search.Contracts.Documents;

public enum SearchDocumentVisibility
{
    Public = 1,
    Authenticated = 2,
    Tenant = 3,
    Permission = 4
}
