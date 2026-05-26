// <copyright file="ChangeTicketStatusRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.TicketManagement.Contracts.Requests;

public sealed class ChangeTicketStatusRequest
{
    public TicketStatusType Status { get; set; }
    public string? Note { get; set; }
}
