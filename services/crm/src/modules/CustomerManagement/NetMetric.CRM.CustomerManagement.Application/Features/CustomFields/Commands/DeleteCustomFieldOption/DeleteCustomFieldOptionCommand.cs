// <copyright file="DeleteCustomFieldOptionCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.CustomerManagement.Application.Features.CustomFields.Commands.DeleteCustomFieldOption;

public sealed class DeleteCustomFieldOptionCommand : IRequest<Unit>
{
    public Guid OptionId { get; init; }
}
