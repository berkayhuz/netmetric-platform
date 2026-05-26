// <copyright file="Error.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Application.Common;

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error NotFound(string resource) => new("not_found", $"{resource} was not found.");

    public static Error Forbidden() => new("forbidden", "The current principal is not allowed to perform this operation.");

    public static Error Validation(string message) => new("validation_error", message);

    public static Error Conflict(string message) => new("conflict", message);

    public static Error ReauthenticationRequired(string message = "Recent authentication is required for this operation.")
        => new("reauth_required", message);
}
