// <copyright file="PipelineDefaults.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.PipelineManagement.Application.Common;

public static class PipelineDefaults
{
    public static OpportunityStatusType ResolveStatus(OpportunityStageType stage)
        => stage switch
        {
            OpportunityStageType.Won => OpportunityStatusType.Won,
            OpportunityStageType.Lost => OpportunityStatusType.Lost,
            _ => OpportunityStatusType.Open
        };

    public static decimal ResolveProbability(OpportunityStageType stage)
        => stage switch
        {
            OpportunityStageType.Prospecting => 10m,
            OpportunityStageType.Qualification => 25m,
            OpportunityStageType.NeedsAnalysis => 45m,
            OpportunityStageType.Proposal => 65m,
            OpportunityStageType.Negotiation => 80m,
            OpportunityStageType.Won => 100m,
            OpportunityStageType.Lost => 0m,
            _ => 0m
        };

    public static (string FirstName, string LastName) SplitFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return ("Unknown", "Lead");

        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
            return (parts[0], string.Empty);

        return (parts[0], string.Join(' ', parts.Skip(1)));
    }
}
