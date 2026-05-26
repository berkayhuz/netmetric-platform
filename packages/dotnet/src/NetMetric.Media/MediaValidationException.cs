// <copyright file="MediaValidationException.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Media;

public sealed class MediaValidationException : Exception
{
    public MediaValidationException(string message)
        : base(message)
    {
    }

    public MediaValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
