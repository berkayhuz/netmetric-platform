// <copyright file="PipelineAutomationTrigger.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;
using NetMetric.Entities;

namespace NetMetric.CRM.PipelineManagement.Domain.Entities;

public class PipelineAutomationTrigger : AuditableEntity
{
    public Guid PipelineStageId { get; set; }
    public AutomationTriggerType TriggerType { get; set; }

    public string ActionType { get; set; } = string.Empty; // e.g. "SendEmail", "CreateTask", "UpdateField"
    public string ActionConfiguration { get; set; } = string.Empty; // JSON config for the action
}
