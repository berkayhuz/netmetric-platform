// <copyright file="IImageValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Media.Models;

namespace NetMetric.Media.Abstractions;

public interface IImageValidator
{
    Task<ImageValidationResult> ValidateAsync(string fileName, string declaredContentType, Stream content, long length, CancellationToken cancellationToken);
}
