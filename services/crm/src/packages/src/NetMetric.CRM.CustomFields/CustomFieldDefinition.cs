// <copyright file="CustomFieldDefinition.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.CustomFields;

public class CustomFieldDefinition : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityName
    {
        get => EntityType;
        set => EntityType = value;
    }
    public CustomFieldDataType DataType { get; set; }
    public bool IsRequired { get; set; }
    public bool IsUnique { get; set; }
    public bool IsSystem { get; set; }
    public string? DefaultValue { get; set; }
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public int OrderNo { get; set; }
    public ICollection<CustomFieldOption> Options { get; set; } = [];
}
