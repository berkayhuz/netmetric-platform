// <copyright file="CreateWorkTaskCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.WorkManagement.Contracts.DTOs;

namespace NetMetric.CRM.WorkManagement.Application.Commands.Tasks.CreateWorkTask;

public sealed record CreateWorkTaskCommand(string Title, string Description, Guid? OwnerUserId, DateTime DueAtUtc, int Priority) : IRequest<WorkTaskDto>;
