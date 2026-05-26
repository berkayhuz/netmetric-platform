// <copyright file="CreatePipelineCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.PipelineManagement.Contracts.DTOs;

namespace NetMetric.CRM.PipelineManagement.Application.Commands;

public record CreatePipelineCommand(
    string Name,
    string? Description,
    bool IsDefault,
    int DisplayOrder,
    List<CreatePipelineStageRequest> Stages) : IRequest<PipelineDto>;

public record CreatePipelineStageRequest(
    string Name,
    string? Description,
    int DisplayOrder,
    decimal Probability,
    bool IsWinStage,
    bool IsLostStage);
