// <copyright file="QuoteLineInput.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text;

namespace NetMetric.CRM.QuoteManagement.Application.Common;

public sealed record QuoteLineInput(Guid ProductId, string? Description, int Quantity, decimal UnitPrice, decimal DiscountRate, decimal TaxRate);
