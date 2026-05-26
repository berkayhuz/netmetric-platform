// <copyright file="ExportFileDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.ProductCatalog.Application.Common;

public sealed class ExportFileDto
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required byte[] Content { get; init; }
}
