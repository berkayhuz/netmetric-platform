// <copyright file="ProposalTemplate.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.QuoteManagement.Domain.Entities;

public class ProposalTemplate : AuditableEntity
{
    public string Name { get; set; } = null!;
    public string? SubjectTemplate { get; set; }
    public string BodyTemplate { get; set; } = null!;
    public bool IsDefault { get; set; }
    public string? Notes { get; set; }

    public void SetNotes(string? notes) => Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
}
