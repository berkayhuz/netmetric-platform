// <copyright file="QuoteWorkflowRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text;

namespace NetMetric.CRM.QuoteManagement.Contracts.Requests;

public sealed class QuoteWorkflowRequest
{
    public string? Note { get; set; }
    public string? Reason { get; set; }
    public DateTime? OccurredAt { get; set; }
    public string? RowVersion { get; set; }
}
