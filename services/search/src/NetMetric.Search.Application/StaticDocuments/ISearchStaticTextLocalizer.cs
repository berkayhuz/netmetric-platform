// <copyright file="ISearchStaticTextLocalizer.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Search.Application.StaticDocuments;

public interface ISearchStaticTextLocalizer
{
    IReadOnlyCollection<string> SupportedLocales { get; }

    string ResolveRequired(string key, string locale);

    string? ResolveOptional(string? key, string locale);

    IReadOnlyCollection<string> ResolveKeywords(IReadOnlyCollection<string> keys, string locale);
}
