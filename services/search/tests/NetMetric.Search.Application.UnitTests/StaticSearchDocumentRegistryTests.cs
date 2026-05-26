// <copyright file="StaticSearchDocumentRegistryTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.RegularExpressions;
using FluentAssertions;
using NetMetric.Search.Application.Security;
using NetMetric.Search.Application.StaticDocuments;
using NetMetric.Search.Contracts.Documents;

namespace NetMetric.Search.Application.UnitTests;

public sealed class StaticSearchDocumentRegistryTests
{
    [Fact]
    public async Task Registry_ShouldReturnOneDocumentPerSupportedLocaleForEachManifestItem()
    {
        var registry = new StaticSearchDocumentRegistry();

        var documents = await registry.GetDocumentsAsync(CancellationToken.None);

        documents.Should().HaveCount(StaticSearchDocumentRegistry.ManifestItems.Count * 2);
        documents.Select(document => document.Locale).Distinct().Should().BeEquivalentTo("en-US", "tr-TR");
    }

    [Fact]
    public async Task Registry_ShouldReturnAtLeastOnePublicDocument()
    {
        var registry = new StaticSearchDocumentRegistry();

        var documents = await registry.GetDocumentsAsync(CancellationToken.None);

        documents.Should().Contain(document => document.Visibility == SearchDocumentVisibility.Public);
    }

    [Fact]
    public async Task RegistryDocuments_ShouldPassSecurityValidator()
    {
        var registry = new StaticSearchDocumentRegistry();
        var documents = await registry.GetDocumentsAsync(CancellationToken.None);

        var failures = documents
            .Select(document => new
            {
                document.Id,
                Errors = SearchDocumentSecurityValidator.Validate(document)
            })
            .Where(entry => entry.Errors.Count > 0)
            .ToArray();

        failures.Should().BeEmpty();
    }

    [Fact]
    public async Task Registry_ShouldNotContainCrmPublicDocuments()
    {
        var registry = new StaticSearchDocumentRegistry();
        var documents = await registry.GetDocumentsAsync(CancellationToken.None);

        documents.Should().NotContain(document =>
            document.Source == SearchDocumentSource.Crm &&
            document.Visibility == SearchDocumentVisibility.Public);
    }

    [Fact]
    public async Task Registry_ShouldNotContainAccountPublicDocuments()
    {
        var registry = new StaticSearchDocumentRegistry();
        var documents = await registry.GetDocumentsAsync(CancellationToken.None);

        documents.Should().NotContain(document =>
            document.Source == SearchDocumentSource.Account &&
            document.Visibility == SearchDocumentVisibility.Public);
    }

    [Fact]
    public async Task Registry_ShouldNotContainAuthOrAdminPublicDocuments()
    {
        var registry = new StaticSearchDocumentRegistry();
        var documents = await registry.GetDocumentsAsync(CancellationToken.None);

        documents.Should().NotContain(document =>
            (document.Source == SearchDocumentSource.Auth || document.Source == SearchDocumentSource.Admin) &&
            document.Visibility == SearchDocumentVisibility.Public);
    }

    [Fact]
    public async Task PermissionVisibilityDocuments_ShouldHaveRequiredPermissions()
    {
        var registry = new StaticSearchDocumentRegistry();
        var documents = await registry.GetDocumentsAsync(CancellationToken.None);

        var invalid = documents
            .Where(document => document.Visibility == SearchDocumentVisibility.Permission)
            .Where(document => document.RequiredPermissions.Count == 0)
            .ToArray();

        invalid.Should().BeEmpty();
    }

    [Fact]
    public async Task CrmStaticDocuments_ShouldRemainPermissionGated()
    {
        var registry = new StaticSearchDocumentRegistry();
        var documents = await registry.GetDocumentsAsync(CancellationToken.None);

        var crmDocuments = documents.Where(document => document.Source == SearchDocumentSource.Crm).ToArray();

        crmDocuments.Should().NotBeEmpty();
        crmDocuments.Should().OnlyContain(document => document.Visibility == SearchDocumentVisibility.Permission);
    }

    [Fact]
    public async Task AnonymousVisibleDocuments_ShouldUseAllowedPublicSources()
    {
        var registry = new StaticSearchDocumentRegistry();
        var documents = await registry.GetDocumentsAsync(CancellationToken.None);

        var anonymousVisible = documents
            .Where(document => document.Visibility == SearchDocumentVisibility.Public)
            .ToArray();

        anonymousVisible.Should().OnlyContain(document =>
            !SearchDocumentSecurityValidator.IsPublicSourceBlocked(document.Source));
    }

    [Fact]
    public async Task RegistryUrls_ShouldUseAppLocalPaths_NotLegacySourcePrefixes()
    {
        var registry = new StaticSearchDocumentRegistry();
        var documents = await registry.GetDocumentsAsync(CancellationToken.None);

        documents.Should().NotContain(document =>
            document.Url.StartsWith("/crm/", StringComparison.OrdinalIgnoreCase) ||
            document.Url.StartsWith("/account/", StringComparison.OrdinalIgnoreCase) ||
            document.Url.StartsWith("/tools/", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Registry_ShouldContainRequiredLocalizedCrmAndAccountDocuments()
    {
        var registry = new StaticSearchDocumentRegistry();
        var documents = await registry.GetDocumentsAsync(CancellationToken.None);

        documents.Should().Contain(d => d.Id == "crm-module-customers-en-US" && d.Title == "Customers" && d.Url == "/customers");
        documents.Should().Contain(d => d.Id == "crm-module-customers-tr-TR" && d.Title == "Müşteriler" && d.Url == "/customers");
        documents.Should().Contain(d => d.Id == "crm-module-contacts-en-US" && d.Title == "Contacts" && d.Url == "/contacts");
        documents.Should().Contain(d => d.Id == "crm-module-contacts-tr-TR" && d.Title == "Kişiler" && d.Url == "/contacts");
        documents.Should().Contain(d => d.Id == "account-page-mfa-en-US" && d.Title == "MFA" && d.Url == "/security/mfa");
        documents.Should().Contain(d => d.Id == "account-page-mfa-tr-TR" && d.Title == "MFA" && d.Url == "/security/mfa");
    }

    [Fact]
    public async Task Registry_ShouldKeepKeyAppLocalPaths()
    {
        var registry = new StaticSearchDocumentRegistry();
        var documents = await registry.GetDocumentsAsync(CancellationToken.None);

        documents.Should().Contain(d => d.Id == "crm-module-customers-en-US" && d.Url == "/customers");
        documents.Should().Contain(d => d.Id == "crm-module-contacts-tr-TR" && d.Url == "/contacts");
        documents.Should().Contain(d => d.Id == "account-page-profile-en-US" && d.Url == "/profile");
        documents.Should().Contain(d => d.Id == "account-page-profile-tr-TR" && d.Url == "/profile");
    }

    [Fact]
    public async Task RegistryDocumentIds_ShouldBeMeilisearchSafeAndLocaleSuffixed()
    {
        var registry = new StaticSearchDocumentRegistry();
        var documents = await registry.GetDocumentsAsync(CancellationToken.None);

        documents.Should().OnlyContain(document => Regex.IsMatch(document.Id, "^[A-Za-z0-9-]+$"));
        documents.Should().OnlyContain(document =>
            document.Id.EndsWith("-en-US", StringComparison.Ordinal) ||
            document.Id.EndsWith("-tr-TR", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Registry_ShouldIndexLocalizedKeywordsIntoContentWithoutLocalizingTags()
    {
        var registry = new StaticSearchDocumentRegistry();
        var documents = await registry.GetDocumentsAsync(CancellationToken.None);

        var turkishCustomers = documents.Single(d => d.Id == "crm-module-customers-tr-TR");
        turkishCustomers.Content.Should().Contain("musteriler");
        turkishCustomers.Tags.Should().BeEquivalentTo("crm", "customers", "navigation");
    }

    [Fact]
    public void Factory_ShouldFallbackToEnglish_WhenRequestedLocaleKeyIsMissing()
    {
        var localizer = new SearchStaticTextLocalizer(new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal)
        {
            ["en-US"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["title"] = "English title",
                ["summary"] = "English summary"
            },
            ["tr-TR"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["title"] = "Türkçe başlık"
            }
        });
        var factory = new StaticSearchDocumentFactory(localizer);
        var item = new StaticSearchManifestItem(
            "test-doc",
            SearchDocumentSource.Public,
            "page",
            "title",
            "summary",
            "/test",
            SearchDocumentVisibility.Public);

        var documents = factory.CreateDocuments(item);

        documents.Single(d => d.Locale == "tr-TR").Summary.Should().Be("English summary");
    }

    [Fact]
    public void Factory_ShouldFail_WhenRequiredKeyIsMissingInRequestedAndFallbackLocale()
    {
        var localizer = new SearchStaticTextLocalizer(new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal)
        {
            ["en-US"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["title"] = "English title"
            },
            ["tr-TR"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["title"] = "Türkçe başlık"
            }
        });
        var factory = new StaticSearchDocumentFactory(localizer);
        var item = new StaticSearchManifestItem(
            "test-doc",
            SearchDocumentSource.Public,
            "page",
            "title",
            "missing.summary",
            "/test",
            SearchDocumentVisibility.Public);

        var act = () => factory.CreateDocuments(item);

        act.Should().Throw<KeyNotFoundException>().WithMessage("*missing.summary*");
    }

    [Fact]
    public async Task AuthenticatedAndPermissionVisibility_ShouldRespectAnonymousAccessRules()
    {
        var registry = new StaticSearchDocumentRegistry();
        var documents = await registry.GetDocumentsAsync(CancellationToken.None);
        var anonymous = SearchAccessContext.Anonymous;
        var authenticated = new SearchAccessContext(true, null, []);
        var crmReader = new SearchAccessContext(true, null, ["crm.customer-management.contacts.read"]);

        var accountProfile = documents.Single(d => d.Id == "account-page-profile-en-US");
        var crmContacts = documents.Single(d => d.Id == "crm-module-contacts-en-US");

        SearchDocumentVisibilityEvaluator.CanAccess(accountProfile, anonymous).Should().BeFalse();
        SearchDocumentVisibilityEvaluator.CanAccess(accountProfile, authenticated).Should().BeTrue();
        SearchDocumentVisibilityEvaluator.CanAccess(crmContacts, anonymous).Should().BeFalse();
        SearchDocumentVisibilityEvaluator.CanAccess(crmContacts, crmReader).Should().BeTrue();
    }
}
