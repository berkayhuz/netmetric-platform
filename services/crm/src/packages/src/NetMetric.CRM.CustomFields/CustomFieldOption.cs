// <copyright file="CustomFieldOption.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomFields;

public class CustomFieldOption : AuditableEntity
{
    public Guid CustomFieldDefinitionId { get; set; }
    public CustomFieldDefinition? CustomFieldDefinition { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int OrderNo { get; set; }
}
