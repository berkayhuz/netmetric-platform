// <copyright file="SoftDeleteTicketCategoryCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.TicketManagement.Application.Commands.Categories;

public sealed record SoftDeleteTicketCategoryCommand(Guid TicketCategoryId) : IRequest;
