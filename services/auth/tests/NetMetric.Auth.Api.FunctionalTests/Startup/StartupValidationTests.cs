// <copyright file="StartupValidationTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NetMetric.Auth.API.Security;
using NetMetric.Auth.Application.Options;

namespace NetMetric.Auth.Api.FunctionalTests.Startup;

public sealed class StartupValidationTests
{
    [Fact]
    public void Startup_Should_Fail_In_Production_When_JwtSigningKey_Is_Missing()
    {
        var validation = ValidateJwtOptions(
            new JwtOptions
            {
                Issuer = "https://auth.netmetric.net",
                Audience = "https://app.netmetric.net",
                AccessTokenMinutes = 15,
                RefreshTokenDays = 14,
                SigningKeys = []
            },
            new TestHostEnvironment(Environments.Production));

        validation.Failed.Should().BeTrue();
        validation.Failures.Should().Contain(message => message.Contains("Jwt:SigningKeys", StringComparison.Ordinal));
    }

    [Fact]
    public void Startup_Should_Fail_In_Production_When_TokenTransport_Mode_Is_BodyOnly()
    {
        var validator = new TokenTransportOptionsValidation(new TestHostEnvironment(Environments.Production));
        var result = validator.Validate(
            null,
            new TokenTransportOptions
            {
                Mode = TokenTransportModes.BodyOnly,
                AccessCookieName = "__Secure-netmetric-access",
                RefreshCookieName = "__Secure-netmetric-refresh",
                SessionCookieName = "__Secure-netmetric-session",
                AccessCookiePath = "/",
                RefreshCookiePath = "/api/auth",
                SessionCookiePath = "/api/auth",
                SameSite = "Lax"
            });

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(message => message.Contains("Security:TokenTransport:Mode must be CookiesOnly outside development.", StringComparison.Ordinal));
    }

    [Fact]
    public void Startup_Should_Fail_In_Production_When_Cors_Uses_Wildcard_With_Credentials()
    {
        var validator = new ApiCorsOptionsValidation(new TestHostEnvironment(Environments.Production));
        var result = validator.Validate(
            null,
            new ApiCorsOptions
            {
                AllowedOrigins = ["*"],
                AllowCredentials = true
            });

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(message => message.Contains("AllowedOrigins cannot contain '*'", StringComparison.Ordinal));
    }

    [Fact]
    public void Startup_Should_Accept_Production_NetMetric_Cors_Origins()
    {
        var validator = new ApiCorsOptionsValidation(new TestHostEnvironment(Environments.Production));
        var result = validator.Validate(
            null,
            new ApiCorsOptions
            {
                AllowedOrigins =
                [
                    "https://netmetric.net",
                    "https://www.netmetric.net",
                    "https://auth.netmetric.net",
                    "https://account.netmetric.net",
                    "https://crm.netmetric.net",
                    "https://tools.netmetric.net",
                    "https://api.netmetric.net"
                ],
                AllowCredentials = true
            });

        result.Failed.Should().BeFalse();
    }

    [Fact]
    public void Startup_Should_Reject_Localhost_Cors_Origin_In_Production()
    {
        var validator = new ApiCorsOptionsValidation(new TestHostEnvironment(Environments.Production));
        var result = validator.Validate(
            null,
            new ApiCorsOptions
            {
                AllowedOrigins = ["http://localhost:7002"],
                AllowCredentials = true
            });

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(message => message.Contains("loopback origin", StringComparison.Ordinal));
    }

    [Fact]
    public void Local_Config_Should_Allow_Auth_And_Account_Web_Cookie_Origins()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(FindRepoFile("services/auth/src/NetMetric.Auth.API/appsettings.json")));
        var origins = document.RootElement
            .GetProperty("Security")
            .GetProperty("Cors")
            .GetProperty("AllowedOrigins")
            .EnumerateArray()
            .Select(item => item.GetString())
            .ToArray();

        origins.Should().Contain("http://localhost:7002");
        origins.Should().Contain("http://localhost:7004");
    }

    [Fact]
    public void Production_Config_Should_Mark_Anonymous_Auth_Flows_Tenant_Optional()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(FindRepoFile("services/auth/src/NetMetric.Auth.API/appsettings.Production.json")));
        var prefixes = document.RootElement
            .GetProperty("Security")
            .GetProperty("TenantResolution")
            .GetProperty("TenantOptionalPathPrefixes")
            .EnumerateArray()
            .Select(item => item.GetString())
            .ToArray();

        prefixes.Should().Contain("/api/auth/register");
        prefixes.Should().Contain("/api/auth/login");
        prefixes.Should().Contain("/api/auth/forgot-password");
    }

    [Fact]
    public void Startup_Should_Fail_In_Production_When_IdentityConnection_Is_Loopback()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:IdentityConnection"] = "Server=localhost;Database=AuthDb;User Id=sa;Password=test-only-placeholder;Encrypt=True;"
            })
            .Build();

        var validation = ValidateDatabaseOptions(
            new DatabaseOptions { ApplyMigrationsOnStartup = false },
            new TestHostEnvironment(Environments.Production),
            config);

        validation.Failed.Should().BeTrue();
        validation.Failures.Should().Contain(message => message.Contains("ConnectionStrings:IdentityConnection cannot point to localhost in production.", StringComparison.Ordinal));
    }

    [Fact]
    public void Development_Smtp_InvitationDeliveryOptions_Should_Bind_From_Configuration()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["InvitationDelivery:AcceptBaseUrl"] = "https://localhost:7146",
                ["InvitationDelivery:AcceptPath"] = "/invite/accept",
                ["InvitationDelivery:SenderName"] = "NetMetricApp",
                ["InvitationDelivery:SenderAddress"] = "sender@example.com",
                ["InvitationDelivery:DisableDelivery"] = "false",
                ["InvitationDelivery:Provider"] = "smtp",
                ["InvitationDelivery:SmtpHost"] = "smtp.gmail.com",
                ["InvitationDelivery:SmtpPort"] = "587",
                ["InvitationDelivery:SmtpUseStartTls"] = "true",
                ["InvitationDelivery:SmtpUserName"] = "sender@example.com",
                ["InvitationDelivery:SmtpPassword"] = "test-only-app-password-placeholder",
                ["InvitationDelivery:ResendThrottleSeconds"] = "60",
                ["InvitationDelivery:MaxResends"] = "5"
            })
            .Build();
        var options = config.GetSection(InvitationDeliveryOptions.SectionName).Get<InvitationDeliveryOptions>()!;

        options.Provider.Should().Be("smtp");
        options.SmtpHost.Should().Be("smtp.gmail.com");
        options.SmtpPort.Should().Be(587);
        options.SmtpUseStartTls.Should().BeTrue();
        options.SmtpUserName.Should().Be("sender@example.com");
        options.SmtpPassword.Should().Be("test-only-app-password-placeholder");

        var validation = ValidateInvitationDeliveryOptions(options, new TestHostEnvironment(Environments.Development));
        validation.Failed.Should().BeFalse();
    }

    [Fact]
    public void Startup_Should_Fail_When_Smtp_InvitationDelivery_Is_Enabled_And_Required_Fields_Are_Missing()
    {
        var validation = ValidateInvitationDeliveryOptions(
            new InvitationDeliveryOptions
            {
                AcceptBaseUrl = "https://localhost:7146",
                AcceptPath = "/invite/accept",
                SenderName = "NetMetricApp",
                SenderAddress = "sender@example.com",
                DisableDelivery = false,
                Provider = "smtp",
                SmtpHost = "",
                SmtpPort = 587,
                SmtpUseStartTls = true,
                SmtpUserName = "sender@example.com",
                SmtpPassword = "",
                ResendThrottleSeconds = 60,
                MaxResends = 5
            },
            new TestHostEnvironment(Environments.Development));

        validation.Failed.Should().BeTrue();
        validation.Failures.Should().Contain(message => message.Contains("InvitationDelivery:SmtpHost is required when SMTP delivery is enabled.", StringComparison.Ordinal));
        validation.Failures.Should().Contain(message => message.Contains("InvitationDelivery:SmtpPassword is required when SmtpUserName is configured.", StringComparison.Ordinal));
    }

    [Fact]
    public void Startup_Should_Not_Require_Smtp_Fields_When_InvitationDelivery_Uses_Notification_Pipeline()
    {
        var validation = ValidateInvitationDeliveryOptions(
            new InvitationDeliveryOptions
            {
                AcceptBaseUrl = "https://localhost:7146",
                AcceptPath = "/invite/accept",
                SenderName = "NetMetricApp",
                SenderAddress = "sender@example.com",
                DisableDelivery = false,
                Provider = "notification",
                SmtpHost = "",
                SmtpPort = 587,
                SmtpUseStartTls = true,
                ResendThrottleSeconds = 60,
                MaxResends = 5
            },
            new TestHostEnvironment(Environments.Development));

        validation.Failed.Should().BeFalse();
    }

    private static ValidateOptionsResult ValidateJwtOptions(JwtOptions options, IHostEnvironment environment)
    {
        var validatorType = Type.GetType(
                "NetMetric.Auth.Infrastructure.DependencyInjection.JwtOptionsValidation, NetMetric.Auth.Infrastructure",
                throwOnError: true)!
            ;
        var validator = Activator.CreateInstance(validatorType, environment)!;
        return (ValidateOptionsResult)validatorType.GetMethod("Validate")!.Invoke(validator, [null, options])!;
    }

    private static ValidateOptionsResult ValidateDatabaseOptions(DatabaseOptions options, IHostEnvironment environment, IConfiguration configuration)
    {
        var validatorType = Type.GetType(
                "NetMetric.Auth.Infrastructure.DependencyInjection.DatabaseOptionsValidation, NetMetric.Auth.Infrastructure",
                throwOnError: true)!
            ;
        var validator = Activator.CreateInstance(validatorType, environment, configuration)!;
        return (ValidateOptionsResult)validatorType.GetMethod("Validate")!.Invoke(validator, [null, options])!;
    }

    private static ValidateOptionsResult ValidateInvitationDeliveryOptions(InvitationDeliveryOptions options, IHostEnvironment environment)
    {
        var validatorType = Type.GetType(
                "NetMetric.Auth.Infrastructure.DependencyInjection.InvitationDeliveryOptionsValidation, NetMetric.Auth.Infrastructure",
                throwOnError: true)!
            ;
        var validator = Activator.CreateInstance(validatorType, environment)!;
        return (ValidateOptionsResult)validatorType.GetMethod("Validate")!.Invoke(validator, [null, options])!;
    }

    private static string FindRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository file '{relativePath}'.");
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "NetMetric.Auth.Api.FunctionalTests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
