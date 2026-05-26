// <copyright file="WorkManagementController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.WorkManagement.Application.Commands.Tasks.AssignWorkTaskOwner;
using NetMetric.CRM.WorkManagement.Application.Commands.Meetings.ScheduleMeeting;
using NetMetric.CRM.WorkManagement.Application.Commands.Tasks.CompleteWorkTask;
using NetMetric.CRM.WorkManagement.Application.Commands.Tasks.CreateWorkTask;
using NetMetric.CRM.WorkManagement.Application.Commands.Tasks.DeleteWorkTask;
using NetMetric.CRM.WorkManagement.Application.Commands.Tasks.ReopenWorkTask;
using NetMetric.CRM.WorkManagement.Application.Commands.Tasks.UpdateWorkTask;
using NetMetric.CRM.WorkManagement.Application.Commands.Tasks.UpdateWorkTaskDueDate;
using NetMetric.CRM.WorkManagement.Application.Commands.Tasks.UpdateWorkTaskReminder;
using NetMetric.CRM.WorkManagement.Application.Queries.Tasks.GetWorkTaskById;
using NetMetric.CRM.WorkManagement.Application.Queries.Tasks.GetWorkTasks;
using NetMetric.CRM.WorkManagement.Application.Queries.GetWorkspace;
using NetMetric.CRM.WorkManagement.Contracts.DTOs;
using NetMetric.Pagination;

namespace NetMetric.CRM.API.Controllers.WorkManagement;

[ApiController]
[Route("api/work-management")]
[Authorize(Policy = AuthorizationPolicies.WorkManagementRead)]
public sealed class WorkManagementController(IMediator mediator) : ControllerBase
{
    [HttpGet("workspace")]
    public async Task<IActionResult> GetWorkspace(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetWorkManagementWorkspaceQuery(), cancellationToken));

    [HttpPost("tasks")]
    [Authorize(Policy = AuthorizationPolicies.WorkManagementManage)]
    public async Task<IActionResult> CreateTask([FromBody] CreateWorkTaskRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new CreateWorkTaskCommand(request.Title, request.Description, request.OwnerUserId, request.DueAtUtc, request.Priority), cancellationToken));

    [HttpGet("tasks")]
    public async Task<ActionResult<PagedResult<WorkTaskDto>>> GetTasks(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] Guid? ownerUserId,
        [FromQuery] DateTime? dueFromUtc,
        [FromQuery] DateTime? dueToUtc,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "dueAtUtc",
        [FromQuery] string? sortDirection = "asc",
        CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new GetWorkTasksQuery(search, status, ownerUserId, dueFromUtc, dueToUtc, page, pageSize, sortBy, sortDirection), cancellationToken));

    [HttpGet("tasks/{taskId:guid}")]
    public async Task<ActionResult<WorkTaskDto>> GetTaskById(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await mediator.Send(new GetWorkTaskByIdQuery(taskId), cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPut("tasks/{taskId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.WorkManagementManage)]
    public async Task<ActionResult<WorkTaskDto>> UpdateTask(
        Guid taskId,
        [FromBody] UpdateWorkTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await mediator.Send(new UpdateWorkTaskCommand(taskId, request.Title, request.Description, request.Priority), cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPatch("tasks/{taskId:guid}/complete")]
    [Authorize(Policy = AuthorizationPolicies.WorkManagementManage)]
    public async Task<ActionResult<WorkTaskDto>> CompleteTask(
        Guid taskId,
        [FromBody] CompleteWorkTaskRequest? request,
        CancellationToken cancellationToken = default)
    {
        var task = await mediator.Send(new CompleteWorkTaskCommand(taskId, GetCurrentUserId(), request?.CompletionNote), cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPatch("tasks/{taskId:guid}/reopen")]
    [Authorize(Policy = AuthorizationPolicies.WorkManagementManage)]
    public async Task<ActionResult<WorkTaskDto>> ReopenTask(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await mediator.Send(new ReopenWorkTaskCommand(taskId), cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPatch("tasks/{taskId:guid}/owner")]
    [Authorize(Policy = AuthorizationPolicies.WorkManagementManage)]
    public async Task<ActionResult<WorkTaskDto>> AssignTaskOwner(
        Guid taskId,
        [FromBody] AssignWorkTaskOwnerRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await mediator.Send(new AssignWorkTaskOwnerCommand(taskId, request.OwnerUserId), cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPatch("tasks/{taskId:guid}/due-date")]
    [Authorize(Policy = AuthorizationPolicies.WorkManagementManage)]
    public async Task<ActionResult<WorkTaskDto>> UpdateTaskDueDate(
        Guid taskId,
        [FromBody] UpdateWorkTaskDueDateRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await mediator.Send(new UpdateWorkTaskDueDateCommand(taskId, request.DueAtUtc), cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPatch("tasks/{taskId:guid}/reminder")]
    [Authorize(Policy = AuthorizationPolicies.WorkManagementManage)]
    public async Task<ActionResult<WorkTaskDto>> UpdateTaskReminder(
        Guid taskId,
        [FromBody] UpdateWorkTaskReminderRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await mediator.Send(new UpdateWorkTaskReminderCommand(taskId, request.ReminderAtUtc), cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpDelete("tasks/{taskId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.WorkManagementManage)]
    public async Task<IActionResult> DeleteTask(Guid taskId, CancellationToken cancellationToken = default)
    {
        var deleted = await mediator.Send(new DeleteWorkTaskCommand(taskId), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("meetings")]
    [Authorize(Policy = AuthorizationPolicies.WorkManagementManage)]
    public async Task<IActionResult> ScheduleMeeting([FromBody] ScheduleMeetingRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new ScheduleMeetingCommand(request.Title, request.StartsAtUtc, request.EndsAtUtc, request.OrganizerEmail, request.AttendeeSummary, request.RequiresExternalSync), cancellationToken));

    public sealed record CreateWorkTaskRequest(string Title, string Description, Guid? OwnerUserId, [property: JsonRequired] DateTime DueAtUtc, [property: JsonRequired] int Priority);
    public sealed record UpdateWorkTaskRequest(string Title, string Description, [property: JsonRequired] int Priority);
    public sealed record CompleteWorkTaskRequest(string? CompletionNote);
    public sealed record AssignWorkTaskOwnerRequest(Guid? OwnerUserId);
    public sealed record UpdateWorkTaskDueDateRequest([property: JsonRequired] DateTime DueAtUtc);
    public sealed record UpdateWorkTaskReminderRequest(DateTime? ReminderAtUtc);

    public sealed record ScheduleMeetingRequest(string Title, [property: JsonRequired] DateTime StartsAtUtc, [property: JsonRequired] DateTime EndsAtUtc, string OrganizerEmail, string AttendeeSummary, [property: JsonRequired] bool RequiresExternalSync);

    private Guid? GetCurrentUserId()
    {
        var subject = User.FindFirst("sub")?.Value
            ?? User.FindFirst("oid")?.Value
            ?? User.FindFirst("user_id")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(subject, out var userId) ? userId : null;
    }
}
