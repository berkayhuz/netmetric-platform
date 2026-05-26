// <copyright file="UpdateCustomFieldOptionCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.DTOs.CustomFields;

namespace NetMetric.CRM.CustomerManagement.Application.Features.CustomFields.Commands.UpdateCustomFieldOption;

public sealed record UpdateCustomFieldOptionCommand : IRequest<CustomFieldOptionDto>
{
    public Guid OptionId { get; init; }
    public required string Label { get; init; }
    public required string Value { get; init; }
    public int OrderNo { get; init; }
}
