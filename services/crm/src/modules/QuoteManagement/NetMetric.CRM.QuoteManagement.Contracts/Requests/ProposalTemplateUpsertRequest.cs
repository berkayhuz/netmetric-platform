// <copyright file="ProposalTemplateUpsertRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.QuoteManagement.Contracts.Requests;

public sealed class ProposalTemplateUpsertRequest
{
    public string Name { get; set; } = null!;
    public string? SubjectTemplate { get; set; }
    public string BodyTemplate { get; set; } = null!;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}
