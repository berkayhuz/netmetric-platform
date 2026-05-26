// <copyright file="CreateCustomFieldDefinitionCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.DTOs.CustomFields;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.CustomerManagement.Application.Features.CustomFields.Commands.CreateCustomFieldDefinition;

public sealed record CreateCustomFieldDefinitionCommand : IRequest<CustomFieldDefinitionDto>
{
    public required string Name { get; init; }
    public required string Label { get; init; }
    public required string EntityName { get; init; }
    public CustomFieldDataType DataType { get; init; } = CustomFieldDataType.Text;
    public bool IsRequired { get; init; }
    public bool IsUnique { get; init; }
    public bool IsSystem { get; init; }
    public string? DefaultValue { get; init; }
    public string? Placeholder { get; init; }
    public string? HelpText { get; init; }
    public int OrderNo { get; init; }
}
