// <copyright file="ConsentContracts.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Contracts.Consents;

public sealed record ConsentHistoryItemResponse(
    Guid Id,
    string ConsentType,
    string Version,
    string Status,
    DateTimeOffset DecidedAt);

public sealed record ConsentsResponse(IReadOnlyCollection<ConsentHistoryItemResponse> Items);

public sealed record AcceptConsentRequest(string Version);
