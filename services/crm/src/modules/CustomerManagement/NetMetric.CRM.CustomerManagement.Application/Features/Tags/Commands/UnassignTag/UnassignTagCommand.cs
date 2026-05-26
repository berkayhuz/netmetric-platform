// <copyright file="UnassignTagCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Tags.Commands.UnassignTag;

public sealed class UnassignTagCommand : IRequest<Unit>
{
    public required Guid TagId { get; init; }
    public required string EntityName { get; init; }
    public required Guid EntityId { get; init; }
}
