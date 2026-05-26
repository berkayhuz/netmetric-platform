// <copyright file="MeilisearchSearchQueryServiceTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NetMetric.Search.Application.Queries;
using NetMetric.Search.Application.Security;
using NetMetric.Search.Contracts.Documents;
using NetMetric.Search.Infrastructure.Meilisearch;
using NetMetric.Search.Infrastructure.Meilisearch.Client;
using NetMetric.Search.Infrastructure.Meilisearch.Documents;
using NetMetric.Search.Infrastructure.Meilisearch.Indexing;
using NetMetric.Search.Infrastructure.Meilisearch.Queries;
using NetMetric.Search.Infrastructure.Options;

namespace NetMetric.Search.Application.UnitTests;

public sealed class MeilisearchSearchQueryServiceTests
{
    [Fact]
    public async Task AnonymousSearch_ForPricingLikePublicDocument_ShouldReturnResult()
    {
        var providerPage = new MeilisearchDocumentSearchPage(
            Documents:
            [
                CreateStoredDocument(
                    "public-page-pricing",
                    SearchDocumentSource.Public,
                    SearchDocumentVisibility.Public,
                    type: "page",
                    title: "Pricing",
                    summary: "Pricing information")
            ],
            TotalCount: 1);

        var (service, _, _) = CreateService(providerPage);

        var response = await service.SearchAsync(new SearchRequest(Query: "pricing"), SearchAccessContext.Anonymous, CancellationToken.None);

        response.Items.Should().ContainSingle(item => item.Id == "public-page-pricing");
        response.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task AnonymousSearch_ForCustomersLikeCrmDocument_ShouldReturnNoResults()
    {
        var providerPage = new MeilisearchDocumentSearchPage(
            Documents:
            [
                CreateStoredDocument(
                    "crm-module-customers",
                    SearchDocumentSource.Crm,
                    SearchDocumentVisibility.Permission,
                    type: "navigation",
                    title: "CRM Customers",
                    summary: "Customer records and lifecycle management.",
                    requiredPermissions: ["crm.customer-management.customers.read"])
            ],
            TotalCount: 1);

        var (service, _, _) = CreateService(providerPage);

        var response = await service.SearchAsync(new SearchRequest(Query: "customers"), SearchAccessContext.Anonymous, CancellationToken.None);

        response.Items.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task AnonymousSearch_ShouldDropRestrictedSourcesEvenIfProviderReturnsThem()
    {
        var providerPage = new MeilisearchDocumentSearchPage(
            Documents:
            [
                CreateStoredDocument("public-1", SearchDocumentSource.Public, SearchDocumentVisibility.Public),
                CreateStoredDocument("crm-1", SearchDocumentSource.Crm, SearchDocumentVisibility.Public),
                CreateStoredDocument("account-1", SearchDocumentSource.Account, SearchDocumentVisibility.Public),
                CreateStoredDocument("auth-1", SearchDocumentSource.Auth, SearchDocumentVisibility.Public),
                CreateStoredDocument("admin-1", SearchDocumentSource.Admin, SearchDocumentVisibility.Public)
            ],
            TotalCount: 5);

        var (service, client, _) = CreateService(providerPage);

        var response = await service.SearchAsync(new SearchRequest(Query: "docs"), SearchAccessContext.Anonymous, CancellationToken.None);

        response.Items.Should().HaveCount(1);
        response.Items.Select(item => item.Source).Should().BeEquivalentTo([SearchDocumentSource.Public]);
        response.TotalCount.Should().Be(1);
        client.LastRequest.Should().NotBeNull();
        client.LastRequest!.Filter.Should().Contain("isDeleted = false");
        response.PermissionPostFilteringApplied.Should().BeFalse();
    }

    [Fact]
    public async Task PermissionVisibility_ShouldRequireUserPermission()
    {
        var providerPage = new MeilisearchDocumentSearchPage(
            Documents:
            [
                CreateStoredDocument(
                    "permission-1",
                    SearchDocumentSource.Crm,
                    SearchDocumentVisibility.Permission,
                    requiredPermissions: ["crm.customers.read"],
                    permissionMatchMode: SearchPermissionMatchMode.Any)
            ],
            TotalCount: 1);

        var (service, _, _) = CreateService(providerPage);

        var noPermission = new SearchAccessContext(true, Guid.NewGuid(), []);
        var withPermission = new SearchAccessContext(true, Guid.NewGuid(), ["crm.customers.read"]);

        var deniedResponse = await service.SearchAsync(new SearchRequest(Query: "customer"), noPermission, CancellationToken.None);
        var allowedResponse = await service.SearchAsync(new SearchRequest(Query: "customer"), withPermission, CancellationToken.None);

        deniedResponse.Items.Should().BeEmpty();
        allowedResponse.Items.Should().ContainSingle();
        allowedResponse.PermissionPostFilteringApplied.Should().BeTrue();
    }

    [Fact]
    public async Task PermissionVisibility_ShouldAllowWildcardPermission()
    {
        var providerPage = new MeilisearchDocumentSearchPage(
            Documents:
            [
                CreateStoredDocument(
                    "permission-1",
                    SearchDocumentSource.Crm,
                    SearchDocumentVisibility.Permission,
                    requiredPermissions: ["crm.customer-management.customers.read"],
                    permissionMatchMode: SearchPermissionMatchMode.Any)
            ],
            TotalCount: 1);

        var (service, _, _) = CreateService(providerPage);
        var wildcardAccess = new SearchAccessContext(true, Guid.NewGuid(), ["*"]);

        var response = await service.SearchAsync(new SearchRequest(Query: "customer"), wildcardAccess, CancellationToken.None);

        response.Items.Should().ContainSingle();
        response.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task PermissionVisibility_WithTenantScopedDocument_ShouldRequireMatchingTenantAndPermission()
    {
        var tenantId = Guid.NewGuid();
        var providerPage = new MeilisearchDocumentSearchPage(
            Documents:
            [
                CreateStoredDocument(
                    "crm-customer-1",
                    SearchDocumentSource.Crm,
                    SearchDocumentVisibility.Permission,
                    tenantId: tenantId,
                    requiredPermissions: ["crm.customer-management.customers.read"],
                    permissionMatchMode: SearchPermissionMatchMode.Any,
                    type: "customer")
            ],
            TotalCount: 1);

        var (service, _, _) = CreateService(providerPage);

        var allowedAccess = new SearchAccessContext(true, tenantId, ["crm.customer-management.customers.read"]);
        var wrongTenantAccess = new SearchAccessContext(true, Guid.NewGuid(), ["crm.customer-management.customers.read"]);
        var missingPermissionAccess = new SearchAccessContext(true, tenantId, []);

        var allowedResponse = await service.SearchAsync(new SearchRequest(Query: "customer"), allowedAccess, CancellationToken.None);
        var wrongTenantResponse = await service.SearchAsync(new SearchRequest(Query: "customer"), wrongTenantAccess, CancellationToken.None);
        var missingPermissionResponse = await service.SearchAsync(new SearchRequest(Query: "customer"), missingPermissionAccess, CancellationToken.None);

        allowedResponse.Items.Should().ContainSingle();
        wrongTenantResponse.Items.Should().BeEmpty();
        missingPermissionResponse.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task PermissionVisibility_WithCrossTenantProviderResult_ShouldBePostFilteredWithoutCountLeak()
    {
        var requestTenantId = Guid.NewGuid();
        var providerPage = new MeilisearchDocumentSearchPage(
            Documents:
            [
                CreateStoredDocument(
                    "crm-customer-cross-tenant",
                    SearchDocumentSource.Crm,
                    SearchDocumentVisibility.Permission,
                    tenantId: Guid.NewGuid(),
                    requiredPermissions: ["crm.customer-management.customers.read"],
                    permissionMatchMode: SearchPermissionMatchMode.Any,
                    type: "customer")
            ],
            TotalCount: 1);

        var (service, _, _) = CreateService(providerPage);
        var access = new SearchAccessContext(true, requestTenantId, ["crm.customer-management.customers.read"]);

        var response = await service.SearchAsync(new SearchRequest(Query: "customer"), access, CancellationToken.None);

        response.Items.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task TenantVisibility_ShouldRequireMatchingTenant()
    {
        var tenantId = Guid.NewGuid();
        var providerPage = new MeilisearchDocumentSearchPage(
            Documents:
            [
                CreateStoredDocument(
                    "tenant-1",
                    SearchDocumentSource.Tools,
                    SearchDocumentVisibility.Tenant,
                    tenantId: tenantId)
            ],
            TotalCount: 1);

        var (service, _, _) = CreateService(providerPage);

        var matchingAccess = new SearchAccessContext(true, tenantId, []);
        var nonMatchingAccess = new SearchAccessContext(true, Guid.NewGuid(), []);

        var matching = await service.SearchAsync(new SearchRequest(Query: "tenant"), matchingAccess, CancellationToken.None);
        var nonMatching = await service.SearchAsync(new SearchRequest(Query: "tenant"), nonMatchingAccess, CancellationToken.None);

        matching.Items.Should().ContainSingle();
        nonMatching.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_ShouldRespectNormalizedPageAndPageSize()
    {
        var providerPage = new MeilisearchDocumentSearchPage(
            Documents: [CreateStoredDocument("public-1", SearchDocumentSource.Public, SearchDocumentVisibility.Public)],
            TotalCount: 1);

        var (service, client, _) = CreateService(providerPage);
        var request = new SearchRequest(Query: "x", Page: 0, PageSize: 9999, IncludeDeleted: true);

        _ = await service.SearchAsync(request, SearchAccessContext.Anonymous, CancellationToken.None);

        client.LastRequest.Should().NotBeNull();
        client.LastRequest!.Page.Should().Be(SearchRequestDefaults.DefaultPage);
        client.LastRequest.PageSize.Should().Be(SearchRequestDefaults.MaxPageSize);
        client.LastRequest.Filter.Should().Contain("isDeleted = false");
    }

    private static (MeilisearchSearchQueryService Service, FakeDocumentClient Client, FakeIndexInitializer Initializer) CreateService(
        MeilisearchDocumentSearchPage providerPage)
    {
        var client = new FakeDocumentClient(providerPage);
        var initializer = new FakeIndexInitializer();
        var options = Options.Create(new SearchOptions
        {
            Provider = "Meilisearch",
            IndexName = "searchdocuments"
        });

        var service = new MeilisearchSearchQueryService(
            options,
            initializer,
            client,
            new MeilisearchFilterBuilder(),
            new MeilisearchDocumentMapper(),
            NullLogger<MeilisearchSearchQueryService>.Instance);

        return (service, client, initializer);
    }

    private static MeilisearchSearchDocument CreateStoredDocument(
        string id,
        SearchDocumentSource source,
        SearchDocumentVisibility visibility,
        Guid? tenantId = null,
        IReadOnlyCollection<string>? requiredPermissions = null,
        SearchPermissionMatchMode permissionMatchMode = SearchPermissionMatchMode.Any,
        string type = "page",
        string? title = null,
        string? summary = null)
    {
        return new MeilisearchSearchDocument
        {
            Id = id,
            Source = source.ToString(),
            Type = type,
            Title = title ?? id,
            Summary = summary ?? id,
            Content = "indexed content",
            Url = $"/{id}",
            TenantId = tenantId,
            RequiredPermissions = requiredPermissions ?? [],
            Visibility = visibility.ToString(),
            PermissionMatchMode = permissionMatchMode.ToString(),
            Locale = "en-US",
            Tags = ["tag"],
            Boost = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-3),
            UpdatedAtUtc = DateTimeOffset.UtcNow.AddDays(-2),
            IndexedAtUtc = DateTimeOffset.UtcNow.AddDays(-1),
            IsDeleted = false,
            Metadata = new Dictionary<string, string>()
        };
    }

    private sealed class FakeDocumentClient(MeilisearchDocumentSearchPage providerPage) : IMeilisearchDocumentClient
    {
        public MeilisearchDocumentSearchRequest? LastRequest { get; private set; }

        public Task EnsureIndexExistsAsync(string indexName, string primaryKey, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task ApplyIndexConfigurationAsync(string indexName, MeilisearchIndexConfiguration configuration, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<MeilisearchDocumentSearchPage> SearchAsync(MeilisearchDocumentSearchRequest request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(providerPage);
        }

        public Task UpsertAsync(string indexName, MeilisearchSearchDocument document, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task UpsertManyAsync(string indexName, IReadOnlyCollection<MeilisearchSearchDocument> documents, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<MeilisearchSearchDocument?> GetDocumentAsync(string indexName, string documentId, CancellationToken cancellationToken) =>
            Task.FromResult<MeilisearchSearchDocument?>(null);

        public Task HardDeleteAsync(string indexName, string documentId, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeIndexInitializer : IMeilisearchIndexInitializer
    {
        public int EnsureCalls { get; private set; }

        public Task EnsureInitializedAsync(CancellationToken cancellationToken)
        {
            EnsureCalls++;
            return Task.CompletedTask;
        }
    }
}
