// <copyright file="ProductionFeatureGateMiddleware.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.AspNetCore.ProblemDetails;

namespace NetMetric.CRM.API.Middleware;

public sealed class ProductionFeatureGateMiddleware(
    RequestDelegate next,
    IConfiguration configuration,
    IWebHostEnvironment environment)
{
    private static readonly (PathString Prefix, string ConfigKey, string ModuleName)[] ProductionGates =
    [
        (new PathString("/api/analytics"), "Crm:Features:AnalyticsReportingEnabled", "analytics-reporting"),
        (new PathString("/api/calendar-sync"), "Crm:Features:CalendarSyncEnabled", "calendar-sync"),
        (new PathString("/api/omnichannel"), "Crm:Features:OmnichannelSyncEnabled", "omnichannel-sync")
    ];

    public async Task InvokeAsync(HttpContext context)
    {
        if (!environment.IsDevelopment())
        {
            foreach (var gate in ProductionGates)
            {
                if (context.Request.Path.StartsWithSegments(gate.Prefix) &&
                    !configuration.GetValue<bool>(gate.ConfigKey))
                {
                    await ProblemDetailsSupport.WriteProblemAsync(
                        context,
                        StatusCodes.Status404NotFound,
                        "CRM module is disabled.",
                        $"The {gate.ModuleName} surface is disabled until its production feature flag is explicitly enabled.",
                        errorCode: "crm_module_disabled",
                        cancellationToken: context.RequestAborted);
                    return;
                }
            }

            if (context.Request.Path.StartsWithSegments("/api/integrations") &&
                context.Request.Path.Value?.Contains("/jobs", StringComparison.OrdinalIgnoreCase) == true &&
                !HttpMethods.IsGet(context.Request.Method) &&
                !configuration.GetValue<bool>("Crm:Features:IntegrationJobProcessingEnabled"))
            {
                await ProblemDetailsSupport.WriteProblemAsync(
                    context,
                    StatusCodes.Status404NotFound,
                    "CRM module is disabled.",
                    "The integration-jobs surface is disabled until its production feature flag is explicitly enabled.",
                    errorCode: "crm_module_disabled",
                    cancellationToken: context.RequestAborted);
                return;
            }

            if (context.Request.Path.StartsWithSegments("/api/support-inbox/connections") &&
                context.Request.Path.Value?.Contains("/sync", StringComparison.OrdinalIgnoreCase) == true &&
                !configuration.GetValue<bool>("Crm:Features:SupportInboxSyncEnabled"))
            {
                await ProblemDetailsSupport.WriteProblemAsync(
                    context,
                    StatusCodes.Status404NotFound,
                    "CRM module is disabled.",
                    "The support-inbox-sync surface is disabled until its production feature flag is explicitly enabled.",
                    errorCode: "crm_module_disabled",
                    cancellationToken: context.RequestAborted);
                return;
            }
        }

        await next(context);
    }
}
