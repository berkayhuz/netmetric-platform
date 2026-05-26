// <copyright file="LeadScoringEngineService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Services;
using NetMetric.CRM.Sales;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.LeadManagement.Infrastructure.Services;

public sealed class LeadScoringEngineService(
    ILeadManagementDbContext dbContext,
    ILogger<LeadScoringEngineService> logger) : ILeadScoringEngineService
{
    public async Task EvaluateAndScoreAsync(Guid leadId, CancellationToken cancellationToken)
    {
        var lead = await dbContext.Leads.FirstOrDefaultAsync(x => x.Id == leadId, cancellationToken);
        if (lead is null)
        {
            logger.LogWarning("Lead {LeadId} not found for scoring.", leadId);
            return;
        }

        decimal oldTotalScore = lead.TotalScore;
        decimal newFitScore = CalculateFitScore(lead);

        lead.FitScore = newFitScore;
        lead.TotalScore = lead.FitScore + lead.EngagementScore;
        lead.Grade = EvaluateGrade(lead.TotalScore);

        if (lead.TotalScore != oldTotalScore)
        {
            var leadScoreHistory = new LeadScore
            {
                TenantId = lead.TenantId,
                LeadId = lead.Id,
                Score = lead.TotalScore,
                FitScoreDelta = lead.FitScore - (oldTotalScore - lead.EngagementScore), // Approximation for demo
                EngagementScoreDelta = 0,
                CalculatedAt = DateTime.UtcNow,
                RuleId = "DefaultFitScoringRule_V1",
                ScoreReason = "System periodic scoring based on profile completeness and rules."
            };

            dbContext.LeadScores.Add(leadScoreHistory);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Scored Lead {LeadId}: FitScore={FitScore}, TotalScore={TotalScore}, Grade={Grade}",
            lead.Id, lead.FitScore, lead.TotalScore, lead.Grade);
    }

    private static decimal CalculateFitScore(Lead lead)
    {
        decimal score = 0;

        // Has Email & Phone
        if (!string.IsNullOrWhiteSpace(lead.Email)) score += 10;
        if (!string.IsNullOrWhiteSpace(lead.Phone)) score += 5;

        // Job Title rules
        if (!string.IsNullOrWhiteSpace(lead.JobTitle))
        {
            var title = lead.JobTitle.ToLowerInvariant();
            if (title.Contains("ceo") || title.Contains("cto") || title.Contains("director"))
                score += 20;
            else if (title.Contains("manager"))
                score += 10;
        }

        // Budget evaluation
        if (lead.EstimatedBudget.HasValue && lead.EstimatedBudget.Value > 10000m)
            score += 15;

        // Profile completeness
        if (!string.IsNullOrWhiteSpace(lead.CompanyName)) score += 10;

        return score;
    }

    private static LeadGradeType EvaluateGrade(decimal totalScore)
    {
        return totalScore switch
        {
            >= 80 => LeadGradeType.A,
            >= 60 => LeadGradeType.B,
            >= 40 => LeadGradeType.C,
            >= 20 => LeadGradeType.D,
            _ => LeadGradeType.F
        };
    }
}
