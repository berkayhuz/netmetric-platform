// <copyright file="ValidateQuoteConfigurationQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.QuoteManagement.Contracts.DTOs;

namespace NetMetric.CRM.QuoteManagement.Application.Queries.Quotes;

public sealed record ValidateQuoteConfigurationQuery(Guid QuoteId) : IRequest<CpqValidationResultDto>;
