// <copyright file="BadRequestAppException.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace NetMetric.Exceptions;

public class BadRequestAppException : ValidationException
{
    public BadRequestAppException(string message) : base(message) { }
}
