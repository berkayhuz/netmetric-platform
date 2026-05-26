// <copyright file="LocalToolArtifactStorage.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Options;
using NetMetric.Tools.Application.Abstractions.Storage;
using NetMetric.Tools.Infrastructure.Options;

namespace NetMetric.Tools.Infrastructure.Storage;

public sealed class LocalToolArtifactStorage(IOptions<ToolsArtifactStorageOptions> options) : IToolArtifactStorage
{
    private readonly string _rootPath = Path.GetFullPath(options.Value.RootPath);
    private const string MetadataExtension = ".meta";

    public async Task PutAsync(ToolArtifactWriteRequest request)
    {
        var fullPath = ResolvePath(request.StorageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        request.Content.Position = 0;
        await using var destination = File.Create(fullPath);
        await request.Content.CopyToAsync(destination, request.CancellationToken);
        var metadataPath = fullPath + MetadataExtension;
        await File.WriteAllTextAsync(metadataPath, $"{request.MimeType}\n{request.FileName}\n{request.ChecksumSha256}", request.CancellationToken);
    }

    public async Task<ToolArtifactReadResult?> GetAsync(string storageKey, CancellationToken cancellationToken)
    {
        var fullPath = ResolvePath(storageKey);
        if (!File.Exists(fullPath))
        {
            return null;
        }

        var stream = File.OpenRead(fullPath);
        var mimeType = "application/octet-stream";
        var metadataPath = fullPath + MetadataExtension;
        if (File.Exists(metadataPath))
        {
            var lines = await File.ReadAllLinesAsync(metadataPath, cancellationToken);
            if (lines.Length > 0 && !string.IsNullOrWhiteSpace(lines[0]))
            {
                mimeType = lines[0].Trim();
            }
        }
        await Task.CompletedTask;
        return new ToolArtifactReadResult(stream, mimeType, stream.Length);
    }

    public Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken)
    {
        var fullPath = ResolvePath(storageKey);
        return Task.FromResult(File.Exists(fullPath));
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        var fullPath = ResolvePath(storageKey);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        var metadataPath = fullPath + MetadataExtension;
        if (File.Exists(metadataPath))
        {
            File.Delete(metadataPath);
        }

        return Task.CompletedTask;
    }

    private string ResolvePath(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new ArgumentException("Storage key is required.", nameof(storageKey));
        }

        var normalizedKey = storageKey.Replace('\\', '/').TrimStart('/');
        var combined = Path.GetFullPath(Path.Combine(_rootPath, normalizedKey));

        if (!combined.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Storage key resolved outside the allowed root path.");
        }

        return combined;
    }
}
