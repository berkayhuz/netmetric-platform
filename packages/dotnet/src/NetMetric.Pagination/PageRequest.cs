// <copyright file="PageRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Pagination;

public sealed record PageRequest(int PageNumber, int PageSize)
{
    public int Page => PageNumber;
    public int Size => PageSize;
    public int Skip => (PageNumber - 1) * PageSize;

    public static PageRequest Normalize(int pageNumber, int pageSize)
        => new(Math.Max(1, pageNumber), Math.Clamp(pageSize, 1, 200));

    public static PageRequest Normalize(int? pageNumber, int? pageSize)
        => Normalize(pageNumber ?? 1, pageSize ?? 20);
}
