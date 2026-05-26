// <copyright file="DeleteTagCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Tags.Commands.DeleteTag;

public sealed class DeleteTagCommand : IRequest<Unit>
{
    public Guid TagId { get; init; }
}
