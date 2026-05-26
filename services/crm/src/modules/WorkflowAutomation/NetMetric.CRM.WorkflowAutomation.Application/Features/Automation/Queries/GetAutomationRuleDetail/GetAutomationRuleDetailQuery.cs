// <copyright file="GetAutomationRuleDetailQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.WorkflowAutomation.Contracts.DTOs;

namespace NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Queries.GetAutomationRuleDetail;

public sealed record GetAutomationRuleDetailQuery(Guid TenantId, Guid RuleId) : IRequest<WorkflowRuleDetailDto>;
