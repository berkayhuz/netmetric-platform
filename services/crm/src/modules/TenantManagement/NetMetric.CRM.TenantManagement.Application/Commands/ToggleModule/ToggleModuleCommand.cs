// <copyright file="ToggleModuleCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.TenantManagement.Application.Commands.ToggleModule;

public sealed record ToggleModuleCommand(Guid TenantId, string ModuleKey, bool IsEnabled) : IRequest;
