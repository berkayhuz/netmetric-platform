// <copyright file="IntegrationWebhookIngestionController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Persistence;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Processing;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Providers;
using NetMetric.CRM.IntegrationHub.Domain.Entities;
using NetMetric.CRM.LeadManagement.Application.Commands.Leads;
using NetMetric.CRM.Omnichannel.Application.Abstractions.Persistence;
using NetMetric.CRM.Omnichannel.Domain.Entities;
using NetMetric.CRM.Omnichannel.Domain.Enums;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.API.Controllers.Integrations;

[ApiController]
[Route("api/integrations/webhooks")]
public sealed class IntegrationWebhookIngestionController(
    IIntegrationHubDbContext integrationHubDbContext,
    IOmnichannelDbContext omnichannelDbContext,
    IIntegrationWebhookSecurityService webhookSecurityService,
    IProviderAdapterRegistry providerAdapterRegistry,
    IMediator mediator,
    ILogger<IntegrationWebhookIngestionController> logger) : ControllerBase
{
    private static readonly JsonSerializerOptions CaseInsensitiveJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [HttpGet("meta/{endpointKey}/verify")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyMetaWebhook(
        string endpointKey,
        [FromQuery(Name = "hub.mode")] string? mode,
        [FromQuery(Name = "hub.verify_token")] string? verifyToken,
        [FromQuery(Name = "hub.challenge")] string? challenge,
        CancellationToken cancellationToken)
    {
        var credential = await integrationHubDbContext.ProviderCredentials
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                x => x.EndpointKey == endpointKey &&
                     (x.ProviderKey == "whatsapp" || x.ProviderKey == "instagram") &&
                     x.IsEnabled,
                cancellationToken);

        if (credential is null)
        {
            return NotFound();
        }

        var adapter = providerAdapterRegistry.Resolve(credential.ProviderKey);
        if (adapter is null)
        {
            return NotFound();
        }

        var verifyTokenFromConfig = ReadVerifyTokenFromConfiguration(credential.ConfigurationJson);
        var result = adapter.VerifyChallenge(new ProviderWebhookVerificationInput(
            credential.ProviderKey,
            mode,
            verifyToken,
            challenge,
            null,
            string.Empty,
            new ProviderSecretSet(credential.AccessToken, credential.WebhookSigningSecret, verifyTokenFromConfig)));

        if (!result.IsValid)
        {
            return Forbid();
        }

        return Content(result.ChallengeResponse ?? string.Empty, "text/plain");
    }

    [HttpPost("meta/{endpointKey}")]
    [AllowAnonymous]
    [EnableRateLimiting("crm-webhook")]
    [RequestSizeLimit(262_144)]
    public async Task<IActionResult> ReceiveMetaWebhook(
        string endpointKey,
        [FromBody] JsonElement payload,
        [FromHeader(Name = "X-Hub-Signature-256")] string? hubSignature,
        CancellationToken cancellationToken)
    {
        var credential = await integrationHubDbContext.ProviderCredentials
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                x => x.EndpointKey == endpointKey &&
                     (x.ProviderKey == "whatsapp" || x.ProviderKey == "instagram") &&
                     x.IsEnabled,
                cancellationToken);

        if (credential is null)
        {
            return NotFound();
        }

        var adapter = providerAdapterRegistry.Resolve(credential.ProviderKey);
        if (adapter is null)
        {
            return NotFound();
        }

        var rawPayload = payload.GetRawText();

        var verifyTokenFromConfig = ReadVerifyTokenFromConfiguration(credential.ConfigurationJson);
        var verification = adapter.VerifyPayload(new ProviderWebhookVerificationInput(
            credential.ProviderKey,
            null,
            null,
            null,
            hubSignature,
            rawPayload,
            new ProviderSecretSet(credential.AccessToken, credential.WebhookSigningSecret, verifyTokenFromConfig)));
        if (!verification.IsValid)
        {
            return Unauthorized();
        }

        var normalization = adapter.Normalize(rawPayload);
        if (!normalization.Succeeded)
        {
            return Ok(new { accepted = true, ignored = true, reason = normalization.Code });
        }

        var duplicate = false;
        foreach (var normalized in normalization.Messages)
        {
            var existingDelivery = await integrationHubDbContext.WebhookDeliveries
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    x => x.TenantId == credential.TenantId &&
                         x.ProviderKey == credential.ProviderKey &&
                         x.EventId == normalized.ExternalMessageId,
                    cancellationToken);

            if (existingDelivery is not null)
            {
                duplicate = true;
                continue;
            }

            var signature = hubSignature ?? string.Empty;
            var delivery = new IntegrationWebhookDelivery(
                credential.TenantId,
                credential.ProviderKey,
                normalized.ExternalMessageId,
                webhookSecurityService.ComputeSignatureHash(signature),
                webhookSecurityService.ComputePayloadHash(rawPayload),
                DateTime.UtcNow);
            await integrationHubDbContext.WebhookDeliveries.AddAsync(delivery, cancellationToken);

            var account = await ResolveOrCreateAccountAsync(credential, normalized.ChannelAccountId, cancellationToken);
            var conversation = await ResolveOrCreateConversationAsync(credential, account, normalized, cancellationToken);

            var message = new ChannelMessage(conversation.Id, "inbound", normalized.Text, normalized.SentAtUtc, normalized.ExternalMessageId);
            message.SetSender(normalized.SenderDisplayName);
            message.SetStatus("received");
            message.TenantId = credential.TenantId;
            await omnichannelDbContext.Messages.AddAsync(message, cancellationToken);
            conversation.IncrementUnread();
            delivery.MarkProcessed(DateTime.UtcNow);
        }

        await integrationHubDbContext.SaveChangesAsync(cancellationToken);
        await omnichannelDbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { accepted = true, duplicate });
    }

    [HttpPost("mock/{endpointKey}")]
    [AllowAnonymous]
    [EnableRateLimiting("crm-webhook")]
    [RequestSizeLimit(262_144)]
    public async Task<IActionResult> ReceiveMockWebhook(
        string endpointKey,
        [FromBody] JsonElement payload,
        [FromHeader(Name = "X-NetMetric-Signature")] string? signature,
        [FromHeader(Name = "X-NetMetric-Timestamp")] string? timestamp,
        CancellationToken cancellationToken)
    {
        var credential = await integrationHubDbContext.ProviderCredentials
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.EndpointKey == endpointKey && x.ProviderKey == "mock" && x.IsEnabled, cancellationToken);

        if (credential is null)
        {
            var safeEndpoint = endpointKey.Length <= 12 ? endpointKey : endpointKey[..12];
            logger.LogWarning(
                "Mock webhook endpoint key not found or disabled. EndpointKeyPrefix={EndpointKeyPrefix} Length={Length}",
                safeEndpoint,
                endpointKey.Length);
            return NotFound();
        }

        var rawPayload = payload.GetRawText();
        MockWebhookRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<MockWebhookRequest>(rawPayload, CaseInsensitiveJsonOptions);
        }
        catch (JsonException)
        {
            return BadRequest();
        }

        if (request is null)
        {
            return BadRequest();
        }

        if (string.IsNullOrWhiteSpace(signature) || string.IsNullOrWhiteSpace(timestamp))
        {
            return Unauthorized();
        }

        if (!webhookSecurityService.ValidateSignature(
                credential.WebhookSigningSecret,
                rawPayload,
                timestamp ?? string.Empty,
                signature,
                DateTime.UtcNow,
                TimeSpan.FromMinutes(5)))
        {
            return Unauthorized();
        }

        var existingDelivery = await integrationHubDbContext.WebhookDeliveries
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                x => x.TenantId == credential.TenantId &&
                     x.ProviderKey == "mock" &&
                     x.EventId == request.ExternalMessageId,
                cancellationToken);

        if (existingDelivery is not null)
        {
            return Ok(new { accepted = true, duplicate = true });
        }

        var delivery = new IntegrationWebhookDelivery(
            credential.TenantId,
            "mock",
            request.ExternalMessageId,
            webhookSecurityService.ComputeSignatureHash(signature),
            webhookSecurityService.ComputePayloadHash(rawPayload),
            DateTime.UtcNow);
        await integrationHubDbContext.WebhookDeliveries.AddAsync(delivery, cancellationToken);

        var normalized = new NormalizedInboundMessage(
            "mock",
            request.ExternalMessageId,
            request.ExternalMessageId,
            request.ChannelAccountId,
            request.ExternalConversationId,
            request.SenderExternalId,
            request.SenderDisplayName,
            request.Text,
            request.SentAtUtc);

        var account = await ResolveOrCreateAccountAsync(credential, normalized.ChannelAccountId, cancellationToken);
        var conversation = await ResolveOrCreateConversationAsync(credential, account, normalized, cancellationToken, request.Subject ?? "Mock conversation");

        var message = new ChannelMessage(conversation.Id, "inbound", request.Text, request.SentAtUtc, request.ExternalMessageId);
        message.SetSender(request.SenderDisplayName);
        message.SetStatus("received");
        message.TenantId = credential.TenantId;
        await omnichannelDbContext.Messages.AddAsync(message, cancellationToken);
        conversation.IncrementUnread();

        delivery.MarkProcessed(DateTime.UtcNow);
        await integrationHubDbContext.SaveChangesAsync(cancellationToken);
        await omnichannelDbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { accepted = true, duplicate = false });
    }

    [HttpPost("mock/conversations/{conversationId:guid}/reply")]
    [Authorize(Policy = AuthorizationPolicies.CrmInboxReply)]
    public async Task<IActionResult> Reply(Guid conversationId, [FromBody] ReplyRequest request, CancellationToken cancellationToken)
    {
        var tenantId = User.FindFirst("tenant_id")?.Value;
        if (!Guid.TryParse(tenantId, out var parsedTenantId))
        {
            return Forbid();
        }

        var conversation = await omnichannelDbContext.Conversations
            .FirstOrDefaultAsync(x => x.Id == conversationId && x.TenantId == parsedTenantId, cancellationToken);
        if (conversation is null)
        {
            return NotFound();
        }

        var message = new ChannelMessage(conversation.Id, "outbound", request.Text, DateTime.UtcNow, $"out_{Guid.NewGuid():N}");
        message.SetSender("Agent");
        message.SetStatus("sent");
        await omnichannelDbContext.Messages.AddAsync(message, cancellationToken);
        conversation.MarkActivity(message.SentAtUtc);
        conversation.MarkRead(DateTime.UtcNow);
        await omnichannelDbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { id = message.Id, status = "sent" });
    }

    [HttpPost("mock/conversations/{conversationId:guid}/create-lead")]
    [Authorize(Policy = AuthorizationPolicies.CrmInboxConvert)]
    public async Task<IActionResult> CreateLead(Guid conversationId, [FromBody] CreateLeadFromConversationRequest request, CancellationToken cancellationToken)
    {
        var tenantId = User.FindFirst("tenant_id")?.Value;
        if (!Guid.TryParse(tenantId, out var parsedTenantId))
        {
            return Forbid();
        }

        var conversation = await omnichannelDbContext.Conversations
            .FirstOrDefaultAsync(x => x.Id == conversationId && x.TenantId == parsedTenantId, cancellationToken);
        if (conversation is null)
        {
            return NotFound();
        }

        var lead = await mediator.Send(
            new CreateLeadCommand(
                request.FullName,
                request.CompanyName,
                request.Email,
                request.Phone,
                null,
                $"Created from mock omnichannel conversation {conversation.Id}",
                null,
                null,
                LeadSourceType.Chat,
                LeadStatusType.New,
                PriorityType.Medium,
                null,
                null,
                null),
            cancellationToken);

        conversation.LinkLead(lead.Id);
        await omnichannelDbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { leadId = lead.Id });
    }

    public sealed record MockWebhookRequest(
        string ChannelAccountId,
        string ExternalConversationId,
        string ExternalMessageId,
        string SenderExternalId,
        string SenderDisplayName,
        string Text,
        [property: JsonRequired] DateTime SentAtUtc,
        string? Subject);

    public sealed record ReplyRequest(string Text);
    public sealed record CreateLeadFromConversationRequest(string FullName, string? CompanyName, string? Email, string? Phone);

    private async Task<ChannelAccount> ResolveOrCreateAccountAsync(ProviderCredential credential, string externalAccountId, CancellationToken cancellationToken)
    {
        var account = await omnichannelDbContext.Accounts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                x => x.TenantId == credential.TenantId &&
                     x.ProviderKey == credential.ProviderKey &&
                     x.ProviderCredentialId == credential.Id &&
                     x.ExternalAccountId == externalAccountId,
                cancellationToken);

        if (account is not null)
        {
            return account;
        }

        var providerDisplay = credential.ProviderKey == "mock" ? "Mock Channel" : $"{credential.DisplayName} Channel";
        var routingKey = credential.ProviderKey == "mock" ? "mock.inbox" : $"{credential.ProviderKey}.inbox";
        account = new ChannelAccount(providerDisplay, ChannelType.LiveChat, externalAccountId, $"provider-credential:{credential.Id}", routingKey);
        account.SetProvider(credential.ProviderKey);
        account.LinkProviderCredential(credential.Id);
        account.TenantId = credential.TenantId;
        await omnichannelDbContext.Accounts.AddAsync(account, cancellationToken);
        return account;
    }

    private async Task<ChannelConversation> ResolveOrCreateConversationAsync(
        ProviderCredential credential,
        ChannelAccount account,
        NormalizedInboundMessage normalized,
        CancellationToken cancellationToken,
        string? subject = null)
    {
        var conversation = await omnichannelDbContext.Conversations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                x => x.TenantId == credential.TenantId &&
                     x.AccountId == account.Id &&
                     x.ProviderKey == credential.ProviderKey &&
                     x.ExternalConversationId == normalized.ExternalConversationId,
                cancellationToken);

        if (conversation is not null)
        {
            conversation.MarkActivity(normalized.SentAtUtc);
            return conversation;
        }

        conversation = new ChannelConversation(
            account.Id,
            subject ?? $"{credential.DisplayName} conversation",
            normalized.SenderDisplayName,
            ConversationStatus.Open,
            normalized.SentAtUtc);
        conversation.BindExternal(normalized.ExternalConversationId, normalized.SenderExternalId, credential.ProviderKey);
        conversation.TenantId = credential.TenantId;
        await omnichannelDbContext.Conversations.AddAsync(conversation, cancellationToken);
        return conversation;
    }

    private static string? ReadVerifyTokenFromConfiguration(string? configurationJson)
    {
        if (string.IsNullOrWhiteSpace(configurationJson))
        {
            return null;
        }

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(configurationJson);
            return doc.RootElement.TryGetProperty("verifyToken", out var verifyToken) && verifyToken.ValueKind == System.Text.Json.JsonValueKind.String
                ? verifyToken.GetString()
                : null;
        }
        catch
        {
            return null;
        }
    }

}
