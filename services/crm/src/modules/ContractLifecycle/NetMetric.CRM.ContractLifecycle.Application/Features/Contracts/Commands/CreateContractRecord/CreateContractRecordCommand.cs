// <copyright file="CreateContractRecordCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.ContractLifecycle.Contracts.DTOs;

namespace NetMetric.CRM.ContractLifecycle.Application.Features.Contracts.Commands.CreateContractRecord;

public sealed record CreateContractRecordCommand(
    string Code,
    string Name,
    string? Description) : IRequest<ContractLifecycleSummaryDto>;
