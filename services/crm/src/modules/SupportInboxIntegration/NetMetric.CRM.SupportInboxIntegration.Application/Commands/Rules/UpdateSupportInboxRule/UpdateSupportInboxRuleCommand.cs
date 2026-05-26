// <copyright file="UpdateSupportInboxRuleCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.SupportInboxIntegration.Application.Commands.Rules.UpdateSupportInboxRule;

public sealed record UpdateSupportInboxRuleCommand(Guid RuleId, string Name, string? MatchSender, string? MatchSubjectContains, Guid? AssignToQueueId, Guid? TicketCategoryId, Guid? SlaPolicyId, bool AutoCreateTicket, bool IsActive) : IRequest;
