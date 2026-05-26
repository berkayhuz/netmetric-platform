// <copyright file="GetSlaEscalationRulesQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.TicketSlaManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TicketSlaManagement.Application.Common;
using NetMetric.CRM.TicketSlaManagement.Contracts.DTOs;

namespace NetMetric.CRM.TicketSlaManagement.Application.Queries.Escalations;

public sealed class GetSlaEscalationRulesQueryHandler(ITicketSlaManagementDbContext dbContext) : IRequestHandler<GetSlaEscalationRulesQuery, IReadOnlyList<SlaEscalationRuleDto>>
{
    public async Task<IReadOnlyList<SlaEscalationRuleDto>> Handle(GetSlaEscalationRulesQuery request, CancellationToken cancellationToken) =>
        await dbContext.SlaEscalationRules
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.SlaPolicyId == request.SlaPolicyId)
            .OrderBy(x => x.MetricType)
            .ThenBy(x => x.TriggerBeforeOrAfterMinutes)
            .Select(x => x.ToDto())
            .ToListAsync(cancellationToken);
}
