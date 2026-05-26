// <copyright file="OmnichannelController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.Omnichannel.Application.Abstractions.Persistence;
using NetMetric.CRM.Omnichannel.Application.Commands.Accounts.CreateChannelAccount;
using NetMetric.CRM.Omnichannel.Application.Commands.Sync.TriggerOmnichannelSync;
using NetMetric.CRM.Omnichannel.Application.Queries.GetOmnichannelWorkspace;
using NetMetric.CRM.Omnichannel.Domain.Entities;
using NetMetric.CRM.Omnichannel.Domain.Enums;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.API.Controllers.Omnichannel;

[ApiController]
[Route("api/omnichannel")]
[Authorize(Policy = AuthorizationPolicies.OmnichannelRead)]
public sealed class OmnichannelController(
    IMediator mediator,
    IOmnichannelDbContext dbContext,
    ICustomerManagementDbContext customerManagementDbContext,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("workspace")]
    public async Task<IActionResult> GetWorkspace(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetOmnichannelWorkspaceQuery(), cancellationToken));

    [HttpPost("accounts")]
    [Authorize(Policy = AuthorizationPolicies.OmnichannelManage)]
    public async Task<IActionResult> CreateAccount([FromBody] CreateChannelAccountRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateChannelAccountCommand(request.Name, request.ChannelType, request.ExternalAccountId, request.SecretReference, request.RoutingKey),
            cancellationToken);

        return CreatedAtAction(nameof(GetWorkspace), null, result);
    }

    [HttpPost("accounts/{accountId:guid}/sync")]
    [Authorize(Policy = AuthorizationPolicies.OmnichannelManage)]
    public async Task<IActionResult> TriggerSync(Guid accountId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new TriggerOmnichannelSyncCommand(accountId), cancellationToken));

    [HttpGet("conversations")]
    [Authorize(Policy = AuthorizationPolicies.CrmInboxRead)]
    public async Task<IActionResult> ListConversations(
        [FromQuery] string? status,
        [FromQuery] string? provider,
        [FromQuery] Guid? assignedUserId,
        [FromQuery] bool unreadOnly = false,
        [FromQuery] string? priority = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return ForbidWithError("tenant_context_missing");
        }

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = dbContext.Conversations
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ConversationStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(x => x.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(provider))
        {
            var normalizedProvider = provider.Trim().ToLowerInvariant();
            query = query.Where(x => x.ProviderKey == normalizedProvider);
        }

        if (assignedUserId.HasValue)
        {
            query = query.Where(x => x.AssignedUserId == assignedUserId.Value);
        }

        if (unreadOnly)
        {
            query = query.Where(x => x.UnreadCount > 0);
        }

        if (!string.IsNullOrWhiteSpace(priority) && Enum.TryParse<ConversationPriority>(priority, true, out var parsedPriority))
        {
            query = query.Where(x => x.Priority == parsedPriority);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.LastMessageAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.AccountId,
                x.Subject,
                x.CustomerDisplayName,
                status = x.Status.ToString(),
                priority = x.Priority.ToString(),
                x.UnreadCount,
                x.LastMessageAtUtc,
                x.LeadId,
                x.CustomerId,
                x.AssignedUserId,
                x.AssignedUserDisplayName,
                x.ProviderKey
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            items,
            totalCount,
            pageNumber = page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpGet("conversations/{conversationId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CrmInboxRead)]
    public async Task<IActionResult> GetConversation(Guid conversationId, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return ForbidWithError("tenant_context_missing");
        }

        var conversation = await dbContext.Conversations
            .AsNoTracking()
            .Where(x => x.Id == conversationId && x.TenantId == tenantId)
            .Select(x => new
            {
                x.Id,
                x.AccountId,
                x.Subject,
                x.CustomerDisplayName,
                status = x.Status.ToString(),
                priority = x.Priority.ToString(),
                x.LastMessageAtUtc,
                x.ExternalConversationId,
                x.ExternalParticipantId,
                x.ProviderKey,
                x.UnreadCount,
                x.LastReadAtUtc,
                x.LeadId,
                x.CustomerId,
                x.AssignedUserId,
                x.AssignedUserDisplayName
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (conversation is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = "Conversation not found.", Extensions = { ["errorCode"] = "inbox_conversation_not_found" } });
        }

        var notes = await dbContext.Notes
            .AsNoTracking()
            .Where(x => x.ConversationId == conversationId && x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(100)
            .Select(x => new
            {
                x.Id,
                x.AuthorUserId,
                x.AuthorDisplayName,
                x.NoteText,
                x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(new { conversation, notes });
    }

    [HttpGet("conversations/{conversationId:guid}/messages")]
    [Authorize(Policy = AuthorizationPolicies.CrmInboxRead)]
    public async Task<IActionResult> ListMessages(Guid conversationId, int page = 1, int pageSize = 100, CancellationToken cancellationToken = default)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return ForbidWithError("tenant_context_missing");
        }

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var exists = await dbContext.Conversations.AnyAsync(x => x.Id == conversationId && x.TenantId == tenantId, cancellationToken);
        if (!exists)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = "Conversation not found.", Extensions = { ["errorCode"] = "inbox_conversation_not_found" } });
        }

        var query = dbContext.Messages
            .AsNoTracking()
            .Where(x => x.ConversationId == conversationId && x.TenantId == tenantId)
            .OrderBy(x => x.SentAtUtc);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.Direction,
                x.Body,
                x.SentAtUtc,
                x.ExternalMessageId,
                x.SenderDisplayName,
                x.Status
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            items,
            totalCount,
            pageNumber = page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpPost("conversations/{conversationId:guid}/status")]
    [Authorize(Policy = AuthorizationPolicies.CrmInboxManage)]
    public async Task<IActionResult> UpdateStatus(Guid conversationId, [FromBody] UpdateConversationStatusRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return ForbidWithError("tenant_context_missing");
        }

        if (!Enum.TryParse<ConversationStatus>(request.Status, true, out var status))
        {
            return BadRequest(new ProblemDetails { Status = 400, Title = "Invalid status.", Extensions = { ["errorCode"] = "inbox_invalid_status" } });
        }

        var conversation = await dbContext.Conversations.FirstOrDefaultAsync(x => x.Id == conversationId && x.TenantId == tenantId, cancellationToken);
        if (conversation is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = "Conversation not found.", Extensions = { ["errorCode"] = "inbox_conversation_not_found" } });
        }

        conversation.SetStatus(status);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("conversations/{conversationId:guid}/assign")]
    [Authorize(Policy = AuthorizationPolicies.CrmInboxAssign)]
    public async Task<IActionResult> Assign(Guid conversationId, [FromBody] AssignConversationRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return ForbidWithError("tenant_context_missing");
        }

        if (currentUserService.UserId == Guid.Empty)
        {
            return ForbidWithError("user_context_missing");
        }

        var requestedUserId = request.AssignedUserId ?? currentUserService.UserId;
        if (requestedUserId != currentUserService.UserId)
        {
            return BadRequest(new ProblemDetails
            {
                Status = 400,
                Title = "Only self-assignment is supported in this phase.",
                Extensions = { ["errorCode"] = "inbox_assign_only_self_supported" }
            });
        }

        var conversation = await dbContext.Conversations.FirstOrDefaultAsync(x => x.Id == conversationId && x.TenantId == tenantId, cancellationToken);
        if (conversation is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = "Conversation not found.", Extensions = { ["errorCode"] = "inbox_conversation_not_found" } });
        }

        var displayName = currentUserService.UserName ?? currentUserService.Email ?? "Assigned user";
        conversation.Assign(currentUserService.UserId, displayName);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("conversations/{conversationId:guid}/unassign")]
    [Authorize(Policy = AuthorizationPolicies.CrmInboxAssign)]
    public async Task<IActionResult> Unassign(Guid conversationId, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return ForbidWithError("tenant_context_missing");
        }

        var conversation = await dbContext.Conversations.FirstOrDefaultAsync(x => x.Id == conversationId && x.TenantId == tenantId, cancellationToken);
        if (conversation is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = "Conversation not found.", Extensions = { ["errorCode"] = "inbox_conversation_not_found" } });
        }

        conversation.Unassign();
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("conversations/{conversationId:guid}/mark-read")]
    [Authorize(Policy = AuthorizationPolicies.CrmInboxRead)]
    public async Task<IActionResult> MarkRead(Guid conversationId, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return ForbidWithError("tenant_context_missing");
        }

        var conversation = await dbContext.Conversations.FirstOrDefaultAsync(x => x.Id == conversationId && x.TenantId == tenantId, cancellationToken);
        if (conversation is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = "Conversation not found.", Extensions = { ["errorCode"] = "inbox_conversation_not_found" } });
        }

        conversation.MarkRead(DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("conversations/{conversationId:guid}/note")]
    [Authorize(Policy = AuthorizationPolicies.CrmInboxManage)]
    public async Task<IActionResult> AddNote(Guid conversationId, [FromBody] AddConversationNoteRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return ForbidWithError("tenant_context_missing");
        }

        if (currentUserService.UserId == Guid.Empty)
        {
            return ForbidWithError("user_context_missing");
        }

        var noteText = request.Text?.Trim();
        if (string.IsNullOrWhiteSpace(noteText))
        {
            return BadRequest(new ProblemDetails { Status = 400, Title = "Note text is required.", Extensions = { ["errorCode"] = "inbox_note_required" } });
        }

        if (noteText.Length > 2000)
        {
            return BadRequest(new ProblemDetails { Status = 400, Title = "Note text is too long.", Extensions = { ["errorCode"] = "inbox_note_too_long" } });
        }

        var exists = await dbContext.Conversations.AnyAsync(x => x.Id == conversationId && x.TenantId == tenantId, cancellationToken);
        if (!exists)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = "Conversation not found.", Extensions = { ["errorCode"] = "inbox_conversation_not_found" } });
        }

        var note = new ConversationNote(
            conversationId,
            currentUserService.UserId,
            currentUserService.UserName ?? currentUserService.Email ?? "Internal user",
            noteText,
            DateTime.UtcNow)
        {
            TenantId = tenantId
        };

        await dbContext.Notes.AddAsync(note, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { id = note.Id, note.CreatedAtUtc });
    }

    [HttpPost("conversations/{conversationId:guid}/priority")]
    [Authorize(Policy = AuthorizationPolicies.CrmInboxManage)]
    public async Task<IActionResult> UpdatePriority(Guid conversationId, [FromBody] UpdateConversationPriorityRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return ForbidWithError("tenant_context_missing");
        }

        if (!Enum.TryParse<ConversationPriority>(request.Priority, true, out var priority))
        {
            return BadRequest(new ProblemDetails { Status = 400, Title = "Invalid priority.", Extensions = { ["errorCode"] = "inbox_invalid_priority" } });
        }

        var conversation = await dbContext.Conversations.FirstOrDefaultAsync(x => x.Id == conversationId && x.TenantId == tenantId, cancellationToken);
        if (conversation is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = "Conversation not found.", Extensions = { ["errorCode"] = "inbox_conversation_not_found" } });
        }

        conversation.SetPriority(priority);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("conversations/{conversationId:guid}/link/customer")]
    [Authorize(Policy = AuthorizationPolicies.CrmInboxConvert)]
    public async Task<IActionResult> LinkCustomer(Guid conversationId, [FromBody] LinkConversationCustomerRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return ForbidWithError("tenant_context_missing");
        }

        var conversation = await dbContext.Conversations.FirstOrDefaultAsync(x => x.Id == conversationId && x.TenantId == tenantId, cancellationToken);
        if (conversation is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = "Conversation not found.", Extensions = { ["errorCode"] = "inbox_conversation_not_found" } });
        }

        var customerExists = await customerManagementDbContext.Customers.AnyAsync(
            x => x.Id == request.CustomerId && x.TenantId == tenantId && x.IsActive,
            cancellationToken);
        if (!customerExists)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = "Customer not found.", Extensions = { ["errorCode"] = "inbox_customer_not_found" } });
        }

        conversation.LinkCustomer(request.CustomerId);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("conversations/{conversationId:guid}/link/lead")]
    [Authorize(Policy = AuthorizationPolicies.CrmInboxConvert)]
    public async Task<IActionResult> LinkLead(Guid conversationId, [FromBody] LinkConversationLeadRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return ForbidWithError("tenant_context_missing");
        }

        var conversation = await dbContext.Conversations.FirstOrDefaultAsync(x => x.Id == conversationId && x.TenantId == tenantId, cancellationToken);
        if (conversation is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = "Conversation not found.", Extensions = { ["errorCode"] = "inbox_conversation_not_found" } });
        }

        conversation.LinkLead(request.LeadId);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    public sealed record CreateChannelAccountRequest(string Name, [property: JsonRequired] ChannelType ChannelType, string ExternalAccountId, string SecretReference, string RoutingKey);
    public sealed record UpdateConversationStatusRequest(string Status);
    public sealed record AssignConversationRequest(Guid? AssignedUserId);
    public sealed record AddConversationNoteRequest(string Text);
    public sealed record UpdateConversationPriorityRequest(string Priority);
    public sealed record LinkConversationCustomerRequest([property: JsonRequired] Guid CustomerId);
    public sealed record LinkConversationLeadRequest([property: JsonRequired] Guid LeadId);

    private bool TryGetTenantId(out Guid tenantId)
    {
        tenantId = currentUserService.TenantId;
        return tenantId != Guid.Empty;
    }

    private IActionResult ForbidWithError(string errorCode)
    {
        return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Extensions = { ["errorCode"] = errorCode }
        });
    }
}
