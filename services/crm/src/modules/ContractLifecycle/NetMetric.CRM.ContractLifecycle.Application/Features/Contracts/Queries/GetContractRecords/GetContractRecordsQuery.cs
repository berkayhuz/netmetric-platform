// <copyright file="GetContractRecordsQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.ContractLifecycle.Contracts.DTOs;

namespace NetMetric.CRM.ContractLifecycle.Application.Features.Contracts.Queries.GetContractRecords;

public sealed record GetContractRecordsQuery : IRequest<IReadOnlyList<ContractLifecycleSummaryDto>>;
