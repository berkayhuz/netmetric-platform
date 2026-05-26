// <copyright file="WorkflowAutomationMetrics.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics.Metrics;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Processing;

public static class WorkflowAutomationMetrics
{
    public const string MeterName = "NetMetric.CRM.WorkflowAutomation";

    private static readonly Meter Meter = new(MeterName);
    private static readonly Counter<long> ExecutionsCompleted = Meter.CreateCounter<long>("workflow_rule_executions_completed_total");
    private static readonly Counter<long> ExecutionsRetried = Meter.CreateCounter<long>("workflow_rule_executions_retried_total");
    private static readonly Counter<long> ExecutionsDeadLettered = Meter.CreateCounter<long>("workflow_rule_executions_dead_lettered_total");
    private static readonly Counter<long> ExecutionsSkipped = Meter.CreateCounter<long>("workflow_rule_executions_skipped_total");

    public static void RecordCompleted(string triggerType) => ExecutionsCompleted.Add(1, KeyValuePair.Create<string, object?>("trigger", triggerType));
    public static void RecordRetried(string triggerType) => ExecutionsRetried.Add(1, KeyValuePair.Create<string, object?>("trigger", triggerType));
    public static void RecordDeadLettered(string triggerType) => ExecutionsDeadLettered.Add(1, KeyValuePair.Create<string, object?>("trigger", triggerType));
    public static void RecordSkipped(string triggerType, string reason) => ExecutionsSkipped.Add(1, KeyValuePair.Create<string, object?>("trigger", triggerType), KeyValuePair.Create<string, object?>("reason", reason));
}
