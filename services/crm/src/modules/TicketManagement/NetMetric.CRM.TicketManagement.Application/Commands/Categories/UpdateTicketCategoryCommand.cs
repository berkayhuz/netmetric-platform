// <copyright file="UpdateTicketCategoryCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketManagement.Contracts.DTOs;

namespace NetMetric.CRM.TicketManagement.Application.Commands.Categories;

public sealed record UpdateTicketCategoryCommand(Guid TicketCategoryId, string Name, string? Description, Guid? ParentCategoryId) : IRequest<TicketCategoryDto>;
