// <copyright file="DealPageRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.DealManagement.Application.Common;

public sealed record DealPageRequest(int Page, int PageSize)
{
    public int NormalizedPage => Page < 1 ? 1 : Page;
    public int NormalizedPageSize => PageSize is < 1 or > 200 ? 20 : PageSize;
}
