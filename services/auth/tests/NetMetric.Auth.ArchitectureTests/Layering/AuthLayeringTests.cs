// <copyright file="AuthLayeringTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Reflection;
using System.Xml.Linq;
using FluentAssertions;
using NetArchTest.Rules;
using NetMetric.Auth.API.Controllers;
using NetMetric.Auth.Application.Features.Handlers;
using NetMetric.Auth.Contracts.Requests;
using NetMetric.Auth.Domain.Entities;
using NetMetric.Auth.Infrastructure.Persistence;

namespace NetMetric.Auth.ArchitectureTests.Layering;

public sealed class AuthLayeringTests
{
    private static readonly Assembly DomainAssembly = typeof(User).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(LoginCommandHandler).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(AuthDbContext).Assembly;
    private static readonly Assembly ApiAssembly = typeof(AuthController).Assembly;
    private static readonly Assembly ContractsAssembly = typeof(LoginRequest).Assembly;

    [Fact]
    public void Domain_Should_Not_Depend_On_Outer_Layers()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "NetMetric.Auth.Application",
                "NetMetric.Auth.Infrastructure",
                "NetMetric.Auth.API",
                "Microsoft.AspNetCore",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore.Identity")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(string.Join(Environment.NewLine, result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Api_Or_Infrastructure_Implementations()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("NetMetric.Auth.API", "NetMetric.Auth.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(string.Join(Environment.NewLine, result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Api()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn("NetMetric.Auth.API")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(string.Join(Environment.NewLine, result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Contracts_Should_Not_Depend_On_Infrastructure_Or_Api()
    {
        var result = Types.InAssembly(ContractsAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("NetMetric.Auth.Infrastructure", "NetMetric.Auth.API")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(string.Join(Environment.NewLine, result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Controllers_Should_Not_Depend_On_AuthDbContext()
    {
        var violatingControllers = ApiAssembly
            .GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.Name.EndsWith("Controller", StringComparison.Ordinal))
            .Where(type => type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SelectMany(ctor => ctor.GetParameters())
                .Any(parameter => parameter.ParameterType == typeof(AuthDbContext)))
            .ToArray();

        violatingControllers.Should().BeEmpty();
    }

    [Fact]
    public void Handlers_Should_Not_Depend_On_Controller_Or_HttpContext()
    {
        var violatingHandlers = ApplicationAssembly
            .GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.Name.EndsWith("Handler", StringComparison.Ordinal))
            .Where(type => type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SelectMany(ctor => ctor.GetParameters())
                .Select(parameter => parameter.ParameterType.FullName ?? string.Empty)
                .Any(typeName =>
                    typeName.Contains("ControllerBase", StringComparison.Ordinal) ||
                    typeName.Equals("Microsoft.AspNetCore.Http.HttpContext", StringComparison.Ordinal)))
            .ToArray();

        violatingHandlers.Should().BeEmpty();
    }

    [Fact]
    public void Domain_Entities_Should_Not_Have_AspNet_Or_Persistence_Attributes()
    {
        var violatingTypes = typeof(User).Assembly
            .GetTypes()
            .Where(type => type.IsClass && type.Namespace == "NetMetric.Auth.Domain.Entities")
            .Where(type => type.GetCustomAttributes()
                .Any(attribute =>
                    (attribute.GetType().Namespace ?? string.Empty).StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal) ||
                    (attribute.GetType().Namespace ?? string.Empty).StartsWith("System.ComponentModel.DataAnnotations.Schema", StringComparison.Ordinal)))
            .ToArray();

        violatingTypes.Should().BeEmpty();
    }

    [Fact]
    public void Public_Contracts_Should_Not_Expose_Domain_Entities()
    {
        var domainEntityTypes = DomainAssembly
            .GetTypes()
            .Where(type => type.Namespace == "NetMetric.Auth.Domain.Entities")
            .ToHashSet();

        var violatingContracts = ContractsAssembly
            .GetTypes()
            .Where(type => type.IsPublic && type.Namespace is not null && type.Namespace.StartsWith("NetMetric.Auth.Contracts", StringComparison.Ordinal))
            .Where(type => type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Any(property => domainEntityTypes.Contains(property.PropertyType)))
            .ToArray();

        violatingContracts.Should().BeEmpty();
    }

    [Fact]
    public void Project_References_Should_Follow_Clean_Architecture_Direction()
    {
        var repoRoot = ResolveRepositoryRoot();
        var expectedReferences = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["NetMetric.Auth.Domain.csproj"] = ["NetMetric.Entities.csproj"],
            ["NetMetric.Auth.Contracts.csproj"] = [],
            ["NetMetric.Auth.Application.csproj"] =
            [
                "NetMetric.Auth.Domain.csproj",
                "NetMetric.Auth.Contracts.csproj",
                "NetMetric.AspNetCore.csproj",
                "NetMetric.Clock.csproj"
            ],
            ["NetMetric.Auth.Infrastructure.csproj"] =
            [
                "NetMetric.Auth.Domain.csproj",
                "NetMetric.Auth.Application.csproj",
                "NetMetric.Auth.Contracts.csproj",
                "NetMetric.AspNetCore.csproj",
                "NetMetric.AspNetCore.TrustedGateway.csproj",
                "NetMetric.Messaging.RabbitMq.csproj"
            ],
            ["NetMetric.Auth.API.csproj"] = ["NetMetric.Auth.Application.csproj", "NetMetric.Auth.Infrastructure.csproj", "NetMetric.Auth.Contracts.csproj"]
        };

        foreach (var (projectName, allowedReferences) in expectedReferences)
        {
            var projectPath = Directory
                .GetFiles(Path.Combine(repoRoot, "services", "auth", "src"), projectName, SearchOption.AllDirectories)
                .Single();
            var document = XDocument.Load(projectPath);
            var references = document
                .Descendants("ProjectReference")
                .Select(reference => Path.GetFileName(reference.Attribute("Include")?.Value))
                .Where(reference => !string.IsNullOrWhiteSpace(reference))
                .Cast<string>()
                .ToArray();

            if (allowedReferences.Length == 0)
            {
                references.Should().BeEmpty($"{projectName} should not have project references");
                continue;
            }

            references.Should().OnlyContain(reference => allowedReferences.Contains(reference), $"{projectName} has unexpected project reference");
        }
    }

    private static string ResolveRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "NetMetric.slnx");
            if (File.Exists(candidate))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Repository root could not be resolved for architecture tests.");
    }
}
