// <copyright file="MediaOptionsAndValidationTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text;
using FluentAssertions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using NetMetric.Media;
using NetMetric.Media.Abstractions;
using NetMetric.Media.Models;
using NetMetric.Media.Options;
using NetMetric.Media.Security;
using NetMetric.Media.Services;
using NetMetric.Media.Urls;
using NetMetric.Media.Validation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NetMetric.Account.Api.UnitTests;

public sealed class MediaOptionsAndValidationTests
{
    [Fact]
    public void DevelopmentMediaOptions_ShouldGenerateLocalUrl()
    {
        var options = CreateDevelopmentMediaOptions();
        var validation = new MediaOptionsValidation(new TestHostEnvironment("Development")).Validate(null, options);

        validation.Succeeded.Should().BeTrue();

        var url = new MediaUrlBuilder(Microsoft.Extensions.Options.Options.Create(options))
            .BuildPublicUrl("netmetric/media/tenant/avatar/original.png");

        url.Should().Be("http://localhost:5301/uploads/netmetric/media/tenant/avatar/original.png");
        url.Should().NotContain("cdn.netmetric.net");
        url.Should().NotContain(Path.GetFullPath(options.Local.RootPath));
    }

    [Fact]
    public void ProductionMediaOptions_ShouldAllowCdnBackedCloudflareStorage()
    {
        var options = new MediaOptions
        {
            PublicBaseUrl = "https://cdn.netmetric.net",
            StorageProvider = "CloudflareR2",
            CloudflareR2 = new MediaCloudflareR2Options
            {
                AccountId = "netmetricprodaccount",
                BucketName = "netmetric-prod-media",
                AccessKeyId = "prod-access-key",
                SecretAccessKey = "prod-secret-key",
                ObjectKeyPrefix = "netmetric"
            },
            SecurityScannerProvider = "ClamAv"
        };

        var validation = new MediaOptionsValidation(new TestHostEnvironment("Production")).Validate(null, options);
        validation.Succeeded.Should().BeTrue();

        var url = new MediaUrlBuilder(Microsoft.Extensions.Options.Options.Create(options))
            .BuildPublicUrl("netmetric/media/tenant/avatar/original.png");

        url.Should().Be("https://cdn.netmetric.net/netmetric/media/tenant/avatar/original.png");
    }

    [Fact]
    public async Task MediaAssetService_ShouldRejectInvalidAvatarPayload()
    {
        var options = CreateDevelopmentMediaOptions();
        var storage = new InMemoryMediaStorageProvider();
        var service = new MediaAssetService(
            storage,
            new MediaUrlBuilder(Microsoft.Extensions.Options.Options.Create(options)),
            new DefaultImageValidator(Microsoft.Extensions.Options.Options.Create(options)),
            new NoopMediaSecurityScanner(),
            Microsoft.Extensions.Options.Options.Create(options));

        await using var content = new MemoryStream(Encoding.UTF8.GetBytes("not an image"));
        var request = new MediaUploadRequest(
            "tenant",
            "avatar",
            "user",
            "avatar.png",
            "image/png",
            content,
            content.Length,
            "account");

        var act = () => service.UploadImageAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<MediaValidationException>();
        storage.SavedKeys.Should().BeEmpty();
    }

    [Fact]
    public async Task MediaAssetService_ShouldRejectMimeMismatch()
    {
        var options = CreateDevelopmentMediaOptions();
        var storage = new InMemoryMediaStorageProvider();
        var service = CreateMediaAssetService(options, storage);

        await using var content = new MemoryStream(ValidPngBytes());
        var request = CreateUploadRequest("avatar.png", "image/jpeg", content, content.Length);

        var act = () => service.UploadImageAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<MediaValidationException>()
            .WithMessage("*content type*");
        storage.SavedKeys.Should().BeEmpty();
    }

    [Fact]
    public async Task MediaAssetService_ShouldRejectCorruptPngPayload()
    {
        var options = CreateDevelopmentMediaOptions();
        var storage = new InMemoryMediaStorageProvider();
        var service = CreateMediaAssetService(options, storage);

        await using var content = new MemoryStream([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0]);
        var request = CreateUploadRequest("avatar.png", "image/png", content, content.Length);

        var act = () => service.UploadImageAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<MediaValidationException>();
        storage.SavedKeys.Should().BeEmpty();
    }

    [Fact]
    public async Task MediaAssetService_ShouldAcceptValidPngAndPersistSanitizedImage()
    {
        var options = CreateDevelopmentMediaOptions();
        var storage = new InMemoryMediaStorageProvider();
        var service = CreateMediaAssetService(options, storage);

        await using var content = new MemoryStream(ValidPngBytes());
        var request = CreateUploadRequest("avatar.png", "image/png", content, content.Length);

        var result = await service.UploadImageAsync(request, CancellationToken.None);

        result.ContentType.Should().Be("image/png");
        result.Extension.Should().Be(".png");
        result.Width.Should().Be(1);
        result.Height.Should().Be(1);
        result.SizeBytes.Should().BeGreaterThan(0);
        storage.SavedKeys.Should().ContainSingle();
        storage.SavedPayloads.Should().ContainSingle(payload => payload.Length > 0);
    }

    [Fact]
    public async Task MediaAssetService_ShouldRejectUnsafeScannerResult()
    {
        var options = CreateDevelopmentMediaOptions();
        var storage = new InMemoryMediaStorageProvider();
        var service = CreateMediaAssetService(options, storage, new RejectingMediaSecurityScanner());

        await using var content = new MemoryStream(ValidPngBytes());
        var request = CreateUploadRequest("avatar.png", "image/png", content, content.Length);

        var act = () => service.UploadImageAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<MediaValidationException>()
            .WithMessage("*scanner rejected*");
        storage.SavedKeys.Should().BeEmpty();
    }

    [Fact]
    public void ProductionMediaOptions_ShouldRejectNoopSecurityScannerWhenRequired()
    {
        var options = new MediaOptions
        {
            PublicBaseUrl = "https://cdn.netmetric.net",
            StorageProvider = "CloudflareR2",
            CloudflareR2 = new MediaCloudflareR2Options
            {
                AccountId = "netmetricprodaccount",
                BucketName = "netmetric-prod-media",
                AccessKeyId = "prod-access-key",
                SecretAccessKey = "prod-secret-key"
            },
            SecurityScannerProvider = "Noop",
            RequireSecurityScannerInProduction = true
        };

        var validation = new MediaOptionsValidation(new TestHostEnvironment("Production")).Validate(null, options);

        validation.Succeeded.Should().BeFalse();
        validation.Failures.Should().Contain(failure => failure.Contains("SecurityScannerProvider", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task MediaAssetService_ShouldRejectOversizedFile()
    {
        var options = new MediaOptions
        {
            PublicBaseUrl = "http://localhost:5301/uploads",
            StorageProvider = "LocalFile",
            MaxImageBytes = 8,
            Local = new MediaLocalOptions
            {
                RootPath = ".runlogs/media",
                RequestPath = "/uploads"
            }
        };
        var storage = new InMemoryMediaStorageProvider();
        var service = CreateMediaAssetService(options, storage);

        await using var content = new MemoryStream(ValidPngBytes());
        var request = CreateUploadRequest("avatar.png", "image/png", content, content.Length);

        var act = () => service.UploadImageAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<MediaValidationException>()
            .WithMessage("*size*");
        storage.SavedKeys.Should().BeEmpty();
    }

    private static MediaAssetService CreateMediaAssetService(
        MediaOptions options,
        InMemoryMediaStorageProvider storage,
        IMediaSecurityScanner? scanner = null) =>
        new(
            storage,
            new MediaUrlBuilder(Microsoft.Extensions.Options.Options.Create(options)),
            new DefaultImageValidator(Microsoft.Extensions.Options.Options.Create(options)),
            scanner ?? new NoopMediaSecurityScanner(),
            Microsoft.Extensions.Options.Options.Create(options));

    private static MediaUploadRequest CreateUploadRequest(string fileName, string contentType, Stream content, long length) =>
        new(
            "tenant",
            "avatar",
            "user",
            fileName,
            contentType,
            content,
            length,
            "account");

    private static byte[] ValidPngBytes()
    {
        using var image = new Image<Rgba32>(1, 1);
        using var output = new MemoryStream();
        image.SaveAsPng(output);
        return output.ToArray();
    }

    private static MediaOptions CreateDevelopmentMediaOptions() =>
        new()
        {
            PublicBaseUrl = "http://localhost:5301/uploads",
            StorageProvider = "LocalFile",
            MaxImageBytes = 10 * 1024 * 1024,
            Local = new MediaLocalOptions
            {
                RootPath = ".runlogs/media",
                RequestPath = "/uploads"
            }
        };

    private sealed class InMemoryMediaStorageProvider : NetMetric.Media.Abstractions.IMediaStorageProvider
    {
        public List<string> SavedKeys { get; } = [];
        public List<byte[]> SavedPayloads { get; } = [];

        public string Name => "LocalFile";

        public async Task SaveAsync(string key, Stream content, string contentType, CancellationToken cancellationToken)
        {
            SavedKeys.Add(key);
            using var copy = new MemoryStream();
            await content.CopyToAsync(copy, cancellationToken);
            SavedPayloads.Add(copy.ToArray());
        }

        public Task DeleteAsync(string key, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken) => Task.FromResult(false);
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "NetMetric.Account.Api.UnitTests";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private sealed class RejectingMediaSecurityScanner : IMediaSecurityScanner
    {
        public Task<MediaSecurityScanResult> ScanAsync(MediaSecurityScanRequest request, CancellationToken cancellationToken)
            => Task.FromResult(MediaSecurityScanResult.Unsafe("scanner rejected"));
    }
}
