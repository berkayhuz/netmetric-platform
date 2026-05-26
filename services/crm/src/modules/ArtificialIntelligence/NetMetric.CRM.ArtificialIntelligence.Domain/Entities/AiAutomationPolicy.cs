// <copyright file="AiAutomationPolicy.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.ArtificialIntelligence.Domain.Entities;

public sealed class AiAutomationPolicy : AuditableEntity
{
    private AiAutomationPolicy()
    {
    }

    public AiAutomationPolicy(string name, string triggerName, string actionName, int confidenceThreshold)
    {
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Policy name is required.", nameof(name)) : name.Trim();
        TriggerName = string.IsNullOrWhiteSpace(triggerName) ? throw new ArgumentException("Trigger name is required.", nameof(triggerName)) : triggerName.Trim();
        ActionName = string.IsNullOrWhiteSpace(actionName) ? throw new ArgumentException("Action name is required.", nameof(actionName)) : actionName.Trim();
        ConfidenceThreshold = Math.Clamp(confidenceThreshold, 0, 100);
    }

    public string Name { get; private set; } = null!;
    public string TriggerName { get; private set; } = null!;
    public string ActionName { get; private set; } = null!;
    public int ConfidenceThreshold { get; private set; }
}
