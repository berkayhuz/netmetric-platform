// <copyright file="CreateSupportInboxRuleCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.SupportInboxIntegration.Application.Commands.Rules.CreateSupportInboxRule;

public sealed record CreateSupportInboxRuleCommand(Guid ConnectionId, string Name, string? MatchSender, string? MatchSubjectContains, Guid? AssignToQueueId, Guid? TicketCategoryId, Guid? SlaPolicyId, bool AutoCreateTicket) : IRequest;
