// <copyright file="GetExecutionLogDetailQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.WorkflowAutomation.Contracts.DTOs;

namespace NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Queries.GetExecutionLogDetail;

public sealed record GetExecutionLogDetailQuery(Guid TenantId, Guid ExecutionLogId) : IRequest<WorkflowExecutionLogDetailDto>;
