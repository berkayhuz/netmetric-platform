// <copyright file="IToolsUploadSecurityValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Tools.Application.Abstractions.Security;

public sealed record ToolsUploadValidationRequest(
    string DeclaredMimeType,
    string OriginalFileName,
    long ContentLength,
    IReadOnlyCollection<string> AllowedMimeTypes,
    Stream Content);

public sealed record ToolsUploadValidationResult(
    string DetectedMimeType,
    string SafeFileName,
    string ChecksumSha256);

public interface IToolsUploadSecurityValidator
{
    Task<ToolsUploadValidationResult> ValidateAsync(ToolsUploadValidationRequest request, CancellationToken cancellationToken);
}
