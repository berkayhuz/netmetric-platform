// <copyright file="ToolsUploadSecurityValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Buffers.Binary;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using NetMetric.Tools.Application.Abstractions.Security;
using NetMetric.Tools.Application.Common;
using NetMetric.Tools.Infrastructure.Options;

namespace NetMetric.Tools.Infrastructure.Security;

public sealed class ToolsUploadSecurityValidator(IOptions<ToolsUploadSecurityOptions> options) : IToolsUploadSecurityValidator
{
    private readonly ToolsUploadSecurityOptions _options = options.Value;

    public async Task<ToolsUploadValidationResult> ValidateAsync(ToolsUploadValidationRequest request, CancellationToken cancellationToken)
    {
        if (request.ContentLength <= 0 || request.ContentLength > _options.MaxUploadBytes)
        {
            throw new InvalidOperationException("Upload size is outside allowed limits.");
        }

        var safeName = SafeFileName.Normalize(request.OriginalFileName);
        var detectedMime = DetectMime(request.Content) ?? throw new InvalidOperationException("Unsupported or malformed file format.");

        if (!request.AllowedMimeTypes.Contains(detectedMime, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Artifact MIME type is not allowed for this tool.");
        }

        if (!string.Equals(request.DeclaredMimeType?.Trim(), detectedMime, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Declared MIME type does not match file content.");
        }

        ValidateContentStructure(detectedMime, request.Content);
        var checksum = await ComputeSha256Async(request.Content, cancellationToken);
        return new ToolsUploadValidationResult(detectedMime, safeName, checksum);
    }

    private void ValidateContentStructure(string mime, Stream stream)
    {
        stream.Position = 0;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        var data = ms.ToArray();

        if (mime is "image/png")
        {
            var width = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(16, 4));
            var height = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(20, 4));
            ValidateDimensions(width, height);
        }
        else if (mime is "image/jpeg")
        {
            var (w, h) = ParseJpegDimensions(data);
            ValidateDimensions(w, h);
        }
        else if (mime is "image/webp")
        {
            var (w, h) = ParseWebpDimensions(data);
            ValidateDimensions(w, h);
        }
        else if (mime is "application/pdf")
        {
            var text = System.Text.Encoding.ASCII.GetString(data);
            var pageCount = text.Split("/Type /Page", StringSplitOptions.None).Length - 1;
            if (pageCount <= 0 || pageCount > _options.MaxPdfPages)
            {
                throw new InvalidOperationException("PDF page limit validation failed.");
            }
        }

        stream.Position = 0;
    }

    private void ValidateDimensions(int width, int height)
    {
        if (width <= 0 || height <= 0 || width > _options.MaxImageWidth || height > _options.MaxImageHeight)
        {
            throw new InvalidOperationException("Image dimensions exceed allowed limits.");
        }
    }

    private static string? DetectMime(Stream stream)
    {
        stream.Position = 0;
        Span<byte> header = stackalloc byte[16];
        var read = stream.Read(header);
        stream.Position = 0;

        if (read >= 8 &&
            header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
            header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
        {
            return "image/png";
        }

        if (read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) return "image/jpeg";

        if (read >= 12 &&
            header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
            header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
        {
            return "image/webp";
        }

        if (read >= 5 && header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46 && header[4] == 0x2D) return "application/pdf";

        return null;
    }

    private static async Task<string> ComputeSha256Async(Stream stream, CancellationToken cancellationToken)
    {
        stream.Position = 0;
        using var hasher = SHA256.Create();
        var hash = await hasher.ComputeHashAsync(stream, cancellationToken);
        stream.Position = 0;
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static (int Width, int Height) ParseJpegDimensions(byte[] data)
    {
        var i = 2;
        while (i + 8 < data.Length)
        {
            if (data[i] != 0xFF) { i++; continue; }
            var marker = data[i + 1];
            var len = (data[i + 2] << 8) + data[i + 3];
            if (len < 2 || i + len + 1 >= data.Length) break;
            if (marker is 0xC0 or 0xC2)
            {
                var h = (data[i + 5] << 8) + data[i + 6];
                var w = (data[i + 7] << 8) + data[i + 8];
                return (w, h);
            }
            i += 2 + len;
        }
        throw new InvalidOperationException("Malformed JPEG image.");
    }

    private static (int Width, int Height) ParseWebpDimensions(byte[] data)
    {
        var chunk = System.Text.Encoding.ASCII.GetString(data, 12, Math.Min(4, data.Length - 12));
        if (chunk == "VP8X" && data.Length >= 30)
        {
            var w = 1 + data[24] + (data[25] << 8) + (data[26] << 16);
            var h = 1 + data[27] + (data[28] << 8) + (data[29] << 16);
            return (w, h);
        }

        throw new InvalidOperationException("Malformed WebP image.");
    }
}
