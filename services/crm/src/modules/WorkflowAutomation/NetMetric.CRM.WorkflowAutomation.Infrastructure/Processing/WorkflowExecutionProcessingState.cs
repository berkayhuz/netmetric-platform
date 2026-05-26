// <copyright file="WorkflowExecutionProcessingState.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Options;
using NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Rules;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Processing;

public sealed class WorkflowExecutionProcessingState(IOptions<WorkflowAutomationOptions> options) : IWorkflowExecutionProcessingState
{
    public bool IsEnabled => options.Value.WorkerEnabled;
}
