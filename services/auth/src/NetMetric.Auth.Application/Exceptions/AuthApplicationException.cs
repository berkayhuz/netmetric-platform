// <copyright file="AuthApplicationException.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Application.Exceptions;

public sealed class AuthApplicationException(
    string title,
    string detail,
    int statusCode,
    string? errorCode = null,
    string? type = null) : Exception(detail)
{
    public string Title { get; } = title;
    public int StatusCode { get; } = statusCode;
    public string? ErrorCode { get; } = errorCode;
    public string Type { get; } = type ?? $"https://httpstatuses.com/{statusCode}";
}
