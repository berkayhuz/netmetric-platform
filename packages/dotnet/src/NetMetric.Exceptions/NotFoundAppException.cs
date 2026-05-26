// <copyright file="NotFoundAppException.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Exceptions;

public class NotFoundAppException : NotFoundException
{
    public NotFoundAppException(string message) : base(message) { }
    public NotFoundAppException(string name, object key) : base(name, key) { }
}
