// <copyright file="TicketCategoryDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.TicketManagement.Contracts.DTOs;

public sealed record TicketCategoryDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    bool IsActive);
