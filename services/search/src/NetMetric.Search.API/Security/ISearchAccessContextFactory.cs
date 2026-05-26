// <copyright file="ISearchAccessContextFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Security.Claims;
using NetMetric.Search.Application.Security;

namespace NetMetric.Search.API.Security;

public interface ISearchAccessContextFactory
{
    SearchAccessContext Create(ClaimsPrincipal? principal);
}
