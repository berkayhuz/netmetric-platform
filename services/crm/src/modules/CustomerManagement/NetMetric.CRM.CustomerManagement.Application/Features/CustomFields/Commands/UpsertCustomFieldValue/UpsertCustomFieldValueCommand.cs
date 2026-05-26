// <copyright file="UpsertCustomFieldValueCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.DTOs.CustomFields;

namespace NetMetric.CRM.CustomerManagement.Application.Features.CustomFields.Commands.UpsertCustomFieldValue;

public sealed class UpsertCustomFieldValueCommand : IRequest<CustomFieldValueDto>
{
    public required Guid DefinitionId { get; init; }
    public required string EntityName { get; init; }
    public required Guid EntityId { get; init; }
    public string? Value { get; init; }
}
