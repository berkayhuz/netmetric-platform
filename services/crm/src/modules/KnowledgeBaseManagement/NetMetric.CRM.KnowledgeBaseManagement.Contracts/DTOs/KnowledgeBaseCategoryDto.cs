// <copyright file="KnowledgeBaseCategoryDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.KnowledgeBaseManagement.Contracts.DTOs;

public sealed record KnowledgeBaseCategoryDto(Guid Id, string Name, string Slug, string? Description, int SortOrder);
