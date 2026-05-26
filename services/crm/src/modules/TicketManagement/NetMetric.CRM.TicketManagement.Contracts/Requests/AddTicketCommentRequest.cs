// <copyright file="AddTicketCommentRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.TicketManagement.Contracts.Requests;

public sealed class AddTicketCommentRequest
{
    public string Comment { get; set; } = null!;
    public bool IsInternal { get; set; }
}
