// <copyright file="CreateClassificationSchemeCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TagManagement.Contracts.DTOs;

namespace NetMetric.CRM.TagManagement.Application.Features.Classifications.Commands.CreateClassificationScheme;

public sealed record CreateClassificationSchemeCommand(string Name, string EntityType) : IRequest<Guid>;
