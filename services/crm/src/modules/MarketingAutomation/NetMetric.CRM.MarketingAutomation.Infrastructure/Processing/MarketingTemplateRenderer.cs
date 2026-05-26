// <copyright file="MarketingTemplateRenderer.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.RegularExpressions;
using NetMetric.CRM.MarketingAutomation.Application.Abstractions;
using NetMetric.CRM.MarketingAutomation.Contracts.DTOs;

namespace NetMetric.CRM.MarketingAutomation.Infrastructure.Processing;

public sealed partial class MarketingTemplateRenderer : IMarketingTemplateRenderer
{
    public MarketingTemplatePreviewDto Render(string subject, string htmlBody, string textBody, string payloadJson)
        => new(RenderText(subject, payloadJson), RenderText(htmlBody, payloadJson), RenderText(textBody, payloadJson));

    private static string RenderText(string value, string payloadJson)
        => TokenPattern().Replace(value, match =>
        {
            var field = match.Groups["field"].Value;
            return MarketingUtilities.ReadString(payloadJson, field) ?? string.Empty;
        });

    [GeneratedRegex("\\{\\{\\s*(?<field>[A-Za-z0-9_.-]+)\\s*\\}\\}")]
    private static partial Regex TokenPattern();
}
