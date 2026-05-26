// <copyright file="IMediaAssetService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Media.Models;

namespace NetMetric.Media.Abstractions;

public interface IMediaAssetService
{
    Task<MediaUploadResult> UploadImageAsync(MediaUploadRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken);
}
