// <copyright file="ValidationAppException.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Exceptions;

public class ValidationAppException : ValidationException
{
    public ValidationAppException(string message) : base(message) { }
    public ValidationAppException(string message, IDictionary<string, string[]> errors) : base(message, errors) { }
}
