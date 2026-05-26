// <copyright file="ToolsInfrastructureServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetMetric.Tools.Application.Abstractions.Security;
using NetMetric.Tools.Application.Abstractions.Storage;
using NetMetric.Tools.Infrastructure.Options;
using NetMetric.Tools.Infrastructure.Security;
using NetMetric.Tools.Infrastructure.Storage;

namespace NetMetric.Tools.Infrastructure.DependencyInjection;

public static class ToolsInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddToolsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<ICurrentUserAccessor, HttpCurrentUserAccessor>();

        services
            .AddOptions<ToolsArtifactStorageOptions>()
            .Bind(configuration.GetSection(ToolsArtifactStorageOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.RootPath), "Artifact storage root path is required.")
            .ValidateOnStart();
        services
            .AddOptions<ToolsUploadSecurityOptions>()
            .Bind(configuration.GetSection(ToolsUploadSecurityOptions.SectionName))
            .ValidateOnStart();
        services
            .AddOptions<ToolsRetentionOptions>()
            .Bind(configuration.GetSection(ToolsRetentionOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IToolsUploadSecurityValidator, ToolsUploadSecurityValidator>();
        services.AddSingleton<IToolsFileSecurityScanner, NoOpToolsFileSecurityScanner>();
        services.AddHostedService<ToolsArtifactCleanupService>();
        services.AddSingleton<IToolArtifactStorage>(sp =>
        {
            var env = sp.GetRequiredService<IHostEnvironment>();
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ToolsArtifactStorageOptions>>().Value;
            if (env.IsProduction() && string.Equals(options.Provider, "Local", StringComparison.OrdinalIgnoreCase) && !options.AllowUnsafeLocalInProduction)
            {
                throw new InvalidOperationException("Production cannot use local artifact storage unless AllowUnsafeLocalInProduction=true.");
            }

            return string.Equals(options.Provider, "ObjectStorage", StringComparison.OrdinalIgnoreCase)
                ? new ObjectStorageToolArtifactStorage()
                : new LocalToolArtifactStorage(sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ToolsArtifactStorageOptions>>());
        });

        return services;
    }
}
