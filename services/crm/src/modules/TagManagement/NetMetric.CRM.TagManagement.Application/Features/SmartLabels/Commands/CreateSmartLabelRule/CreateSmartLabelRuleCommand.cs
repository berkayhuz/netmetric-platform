// <copyright file="CreateSmartLabelRuleCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TagManagement.Contracts.DTOs;

namespace NetMetric.CRM.TagManagement.Application.Features.SmartLabels.Commands.CreateSmartLabelRule;

public sealed record CreateSmartLabelRuleCommand(string Name, string EntityType, string ConditionJson) : IRequest<Guid>;
