// <copyright file="CreateTagCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Tags;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Tags.Commands.CreateTag;

public sealed class CreateTagCommand : IRequest<TagDto>
{
    public required string Name { get; init; }
    public string? ColorHex { get; init; }
    public string? Description { get; init; }
}
