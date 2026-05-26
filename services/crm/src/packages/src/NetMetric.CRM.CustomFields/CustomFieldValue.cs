// <copyright file="CustomFieldValue.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomFields;

public class CustomFieldValue : AuditableEntity
{
    public Guid CustomFieldDefinitionId { get; set; }
    public CustomFieldDefinition? Definition { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid DefinitionId
    {
        get => CustomFieldDefinitionId;
        set => CustomFieldDefinitionId = value;
    }
    public string EntityName
    {
        get => EntityType;
        set => EntityType = value;
    }
    public Guid EntityId { get; set; }
    public string? Value { get; set; }
}
