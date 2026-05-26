// <copyright file="ChangeTicketPriorityRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.TicketManagement.Contracts.Requests;

public sealed class ChangeTicketPriorityRequest
{
    public PriorityType Priority { get; set; }
}
