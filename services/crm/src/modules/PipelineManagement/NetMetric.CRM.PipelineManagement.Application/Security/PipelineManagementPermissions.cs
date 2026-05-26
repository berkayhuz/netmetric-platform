// <copyright file="PipelineManagementPermissions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.PipelineManagement.Application.Security;

public static class PipelineManagementPermissions
{
    public const string LostReasonsManage = "pipeline.lost-reasons.manage";
    public const string LostReasonsRead = "pipeline.lost-reasons.read";
    public const string OpportunityPipelineManage = "pipeline.opportunities.manage";
    public const string OpportunityStageHistoryRead = "pipeline.stage-history.read";
    public const string LeadConversionsManage = "pipeline.lead-conversions.manage";
    public const string LeadConversionsRead = "pipeline.lead-conversions.read";

    public const string PipelinesManage = "pipeline.pipelines.manage";
    public const string PipelinesRead = "pipeline.pipelines.read";

    public static readonly string[] All =
    [
        LostReasonsManage,
        LostReasonsRead,
        OpportunityPipelineManage,
        OpportunityStageHistoryRead,
        LeadConversionsManage,
        LeadConversionsRead,
        PipelinesManage,
        PipelinesRead
    ];
}
