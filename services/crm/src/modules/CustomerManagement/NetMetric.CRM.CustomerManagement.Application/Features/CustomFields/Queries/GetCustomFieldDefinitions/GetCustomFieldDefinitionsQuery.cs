// <copyright file="GetCustomFieldDefinitionsQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.DTOs.CustomFields;

namespace NetMetric.CRM.CustomerManagement.Application.Features.CustomFields.Queries.GetCustomFieldDefinitions;

public sealed class GetCustomFieldDefinitionsQuery : IRequest<IReadOnlyList<CustomFieldDefinitionDto>>
{
    public required string EntityName { get; init; }
}
