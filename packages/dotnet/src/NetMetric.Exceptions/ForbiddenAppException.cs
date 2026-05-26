// <copyright file="ForbiddenAppException.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Exceptions;

public class ForbiddenAppException : ForbiddenAccessException
{
    public ForbiddenAppException(string message) : base(message) { }
}
