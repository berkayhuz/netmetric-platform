// <copyright file="CreateTagCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.TagManagement.Application.Features.Tags.Commands.CreateTag;

public sealed record CreateTagCommand(string Name, string Color) : IRequest<Guid>;
