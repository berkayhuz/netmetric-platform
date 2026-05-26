// <copyright file="MediaUploadResult.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Media.Models;

public sealed record MediaUploadResult(
    string ContentType,
    string Extension,
    long SizeBytes,
    string Sha256Hash,
    int? Width,
    int? Height,
    string StorageProvider,
    string StorageKey,
    string PublicUrl);
