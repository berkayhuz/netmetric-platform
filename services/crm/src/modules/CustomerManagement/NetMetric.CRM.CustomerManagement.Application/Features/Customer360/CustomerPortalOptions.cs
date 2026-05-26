// <copyright file="CustomerPortalOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerManagement.Application.Features.Customer360;

public sealed class CustomerPortalOptions
{
    public const string SectionName = "CustomerPortal";
    public string? BaseUrl { get; set; }
    public string TicketsRouteTemplate { get; set; } = "/customers/{customerId}/tickets";
    public string ContractsRouteTemplate { get; set; } = "/customers/{customerId}/contracts";
    public string InvoicesRouteTemplate { get; set; } = "/customers/{customerId}/invoices";
    public string DocumentsRouteTemplate { get; set; } = "/customers/{customerId}/documents";
    public string ProfileRouteTemplate { get; set; } = "/customers/{customerId}";
    public string SupportRouteTemplate { get; set; } = "/support";
}

public static class CustomerPortalLinkBuilder
{
    public static CustomerPortalLinksDto Build(CustomerPortalOptions options, Guid customerId)
    {
        if (string.IsNullOrWhiteSpace(options.BaseUrl) || !Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri))
            return new CustomerPortalLinksDto(null, null, null, null, null, null);

        string Link(string template) => new Uri(baseUri, template.Replace("{customerId}", customerId.ToString("D"), StringComparison.OrdinalIgnoreCase).TrimStart('/')).ToString();
        return new CustomerPortalLinksDto(
            Link(options.TicketsRouteTemplate),
            Link(options.ContractsRouteTemplate),
            Link(options.InvoicesRouteTemplate),
            Link(options.DocumentsRouteTemplate),
            Link(options.ProfileRouteTemplate),
            Link(options.SupportRouteTemplate));
    }
}
