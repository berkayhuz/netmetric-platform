// <copyright file="KnowledgeBaseCategoryUpsertRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.KnowledgeBaseManagement.Contracts.Requests;

public sealed class KnowledgeBaseCategoryUpsertRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}
