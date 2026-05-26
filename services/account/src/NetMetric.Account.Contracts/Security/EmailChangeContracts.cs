// <copyright file="EmailChangeContracts.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Contracts.Security;

public sealed record EmailChangeRequest(string NewEmail, string CurrentPassword);

public sealed record EmailChangeConfirmRequest(string Token);

public sealed record EmailChangeRequestResponse(bool ConfirmationRequired);

public sealed record EmailChangeConfirmResponse(string NewEmail);
