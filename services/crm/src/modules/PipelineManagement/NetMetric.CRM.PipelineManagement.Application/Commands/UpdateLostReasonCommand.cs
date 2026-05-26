// <copyright file="UpdateLostReasonCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.PipelineManagement.Contracts.DTOs;

namespace NetMetric.CRM.PipelineManagement.Application.Commands;

public sealed record UpdateLostReasonCommand(Guid LostReasonId, string Name, string? Description, bool IsDefault, string? RowVersion) : IRequest<LostReasonDto>;
