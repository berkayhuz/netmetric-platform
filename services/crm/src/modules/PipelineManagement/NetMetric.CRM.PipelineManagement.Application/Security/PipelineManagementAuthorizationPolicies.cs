// <copyright file="PipelineManagementAuthorizationPolicies.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.PipelineManagement.Application.Security;

public static class PipelineManagementAuthorizationPolicies
{
    public const string LostReasonsManage = PipelineManagementPermissions.LostReasonsManage;
    public const string LostReasonsRead = PipelineManagementPermissions.LostReasonsRead;
    public const string OpportunityPipelineManage = PipelineManagementPermissions.OpportunityPipelineManage;
    public const string OpportunityStageHistoryRead = PipelineManagementPermissions.OpportunityStageHistoryRead;
    public const string LeadConversionsManage = PipelineManagementPermissions.LeadConversionsManage;
    public const string LeadConversionsRead = PipelineManagementPermissions.LeadConversionsRead;
    public const string PipelinesManage = PipelineManagementPermissions.PipelinesManage;
    public const string PipelinesRead = PipelineManagementPermissions.PipelinesRead;
}
