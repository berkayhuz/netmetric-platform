// <copyright file="ProviderCatalogAndValidationTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Providers;
using NetMetric.CRM.IntegrationHub.Infrastructure.Providers;

namespace NetMetric.CRM.IntegrationHub.UnitTests;

public sealed class ProviderCatalogAndValidationTests
{
    [Fact]
    public void Catalog_Should_Contain_Mock_Whatsapp_Instagram()
    {
        var catalog = CreateCatalog("Development");

        var providers = catalog.List();
        providers.Select(x => x.ProviderKey).Should().Contain(["mock", "whatsapp", "instagram"]);
    }

    [Fact]
    public void Catalog_Should_Hide_Mock_In_Production()
    {
        var catalog = CreateCatalog("Production", mockEnabled: false);

        var providers = catalog.List();

        providers.Select(x => x.ProviderKey).Should().NotContain("mock");
    }

    [Fact]
    public void Production_Config_Should_Fail_When_Mock_Provider_Is_Enabled()
    {
        var validation = new IntegrationProviderCatalogOptionsValidation(new TestHostEnvironment("Production"))
            .Validate(null, new IntegrationProviderCatalogOptions { MockProviderEnabled = true });

        validation.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Development_Config_Should_Allow_Mock_Provider()
    {
        var validation = new IntegrationProviderCatalogOptionsValidation(new TestHostEnvironment("Development"))
            .Validate(null, new IntegrationProviderCatalogOptions { MockProviderEnabled = true });

        validation.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Mock_TestConnection_Should_Succeed()
    {
        var catalog = CreateCatalog("Development");
        var validator = new DefaultProviderCredentialValidator(catalog);
        var tester = new DefaultProviderConnectionTester(catalog, validator, Options.Create(new WhatsAppProviderOptions()));

        var result = tester.Test(new ProviderValidationInput("mock", "Mock", "mock_access_token_sample_123", "mock_signing_secret_sample_1234", null));

        result.Succeeded.Should().BeTrue();
        result.Code.Should().Be("mock_connection_ok");
    }

    [Fact]
    public void WhatsApp_TestConnection_Should_Return_AdapterNotActive()
    {
        var catalog = CreateCatalog("Development");
        var validator = new DefaultProviderCredentialValidator(catalog);
        var tester = new DefaultProviderConnectionTester(catalog, validator, Options.Create(new WhatsAppProviderOptions()));

        var result = tester.Test(new ProviderValidationInput("whatsapp", "WA", "whatsapp_mock_token_sample_123456", "whatsapp_signing_secret_sample_123456", "{\"verifyToken\":\"verify_token_sample_123456\"}"));

        result.Succeeded.Should().BeFalse();
        result.Code.Should().Be("missing_required_fields");
    }

    [Fact]
    public void Missing_Required_Fields_Should_Fail_Validation()
    {
        var catalog = CreateCatalog("Development");
        var validator = new DefaultProviderCredentialValidator(catalog);

        var result = validator.Validate(new ProviderValidationInput("mock", "Mock", "", "", null));

        result.IsValid.Should().BeFalse();
        result.Code.Should().NotBeNullOrWhiteSpace();
    }

    private static DefaultIntegrationProviderCatalog CreateCatalog(string environmentName, bool mockEnabled = true) =>
        new(
            new TestHostEnvironment(environmentName),
            Options.Create(new IntegrationProviderCatalogOptions { MockProviderEnabled = mockEnabled }));

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "NetMetric.CRM.IntegrationHub.UnitTests";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
