// <copyright file="TicketCategoryUpsertRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.TicketManagement.Contracts.Requests;

public sealed class TicketCategoryUpsertRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
}
