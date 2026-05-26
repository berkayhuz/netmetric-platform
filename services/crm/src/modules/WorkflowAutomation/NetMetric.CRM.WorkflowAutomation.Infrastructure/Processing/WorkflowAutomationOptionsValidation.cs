// <copyright file="WorkflowAutomationOptionsValidation.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Processing;

public sealed class WorkflowAutomationOptionsValidation(IHostEnvironment environment) : IValidateOptions<WorkflowAutomationOptions>
{
    public ValidateOptionsResult Validate(string? name, WorkflowAutomationOptions options)
    {
        var failures = new List<string>();

        if (options.BatchSize <= 0)
        {
            failures.Add("Crm:WorkflowAutomation:BatchSize must be greater than 0.");
        }

        if (options.WebhookRequestTimeoutSeconds is < 1 or > 300)
        {
            failures.Add("Crm:WorkflowAutomation:WebhookRequestTimeoutSeconds must be between 1 and 300.");
        }

        if (options.WebhookMaxResponseBytes is < 1024 or > 1048576)
        {
            failures.Add("Crm:WorkflowAutomation:WebhookMaxResponseBytes must be between 1024 and 1048576.");
        }

        if (options.UseWebhookProxy)
        {
            failures.Add("Crm:WorkflowAutomation:UseWebhookProxy is not supported for workflow webhook outbound calls. Configure explicit egress infrastructure instead.");
        }

        foreach (var host in options.WebhookAllowedHosts)
        {
            if (!WebhookOutboundRequestValidator.IsAllowedHostPattern(host))
            {
                failures.Add($"Crm:WorkflowAutomation:WebhookAllowedHosts contains invalid host pattern '{host}'.");
            }
        }

        if (environment.IsProduction())
        {
            if (options.AllowHttpWebhookTargets)
            {
                failures.Add("Crm:WorkflowAutomation:AllowHttpWebhookTargets must be false in production.");
            }

            if ((options.EngineEnabled || options.WorkerEnabled) && options.WebhookAllowedHosts.Length == 0)
            {
                failures.Add("Crm:WorkflowAutomation:WebhookAllowedHosts must contain at least one host in production.");
            }
        }

        return failures.Count > 0 ? ValidateOptionsResult.Fail(failures) : ValidateOptionsResult.Success;
    }
}
