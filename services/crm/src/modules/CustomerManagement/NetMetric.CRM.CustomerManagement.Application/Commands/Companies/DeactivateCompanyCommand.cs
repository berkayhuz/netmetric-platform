// <copyright file="DeactivateCompanyCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.CustomerManagement.Application.Commands.Companies;

public sealed record DeactivateCompanyCommand(Guid CompanyId) : IRequest<Unit>;
