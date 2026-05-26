// <copyright file="DetectDuplicatesCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerIntelligence.Contracts.DTOs;

namespace NetMetric.CRM.CustomerIntelligence.Application.Features.Duplicates.Commands.DetectDuplicates;

public sealed record DetectDuplicatesCommand(Guid SubjectId, string EntityType) : IRequest<Guid>;
