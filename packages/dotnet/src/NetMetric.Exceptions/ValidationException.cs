// <copyright file="ValidationException.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Exceptions;

public class ValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = new Dictionary<string, string[]>();

    public ValidationException(string message) : base(message) { }
    public ValidationException(string message, IDictionary<string, string[]> errors) : base(message)
        => Errors = new Dictionary<string, string[]>(errors);
}
