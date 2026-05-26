// <copyright file="ConflictAppException.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Exceptions;

public class ConflictAppException : Exception
{
    public ConflictAppException(string message) : base(message) { }
}
