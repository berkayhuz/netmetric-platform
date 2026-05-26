// <copyright file="ProviderAdaptersTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Providers;
using NetMetric.CRM.IntegrationHub.Infrastructure.Providers;

namespace NetMetric.CRM.IntegrationHub.UnitTests;

public sealed class ProviderAdaptersTests
{
    private const string SampleAccessValue = "sample-access-value";
    private const string SampleSigningValue = "sample-signing-value";
    private const string SampleVerifyValue = "sample-verify-value";

    [Fact]
    public void WhatsApp_Challenge_Should_Succeed_When_VerifyToken_Matches()
    {
        var adapter = CreateWhatsAppAdapter();
        var result = adapter.VerifyChallenge(new ProviderWebhookVerificationInput(
            "whatsapp",
            "subscribe",
            SampleVerifyValue,
            "challenge-value",
            null,
            string.Empty,
            new ProviderSecretSet(SampleAccessValue, SampleSigningValue, SampleVerifyValue)));

        result.IsValid.Should().BeTrue();
        result.ChallengeResponse.Should().Be("challenge-value");
    }

    [Fact]
    public void WhatsApp_Signature_Should_Fail_When_Invalid()
    {
        var adapter = CreateWhatsAppAdapter();
        var payload = "{\"object\":\"whatsapp_business_account\"}";
        var result = adapter.VerifyPayload(new ProviderWebhookVerificationInput(
            "whatsapp",
            null,
            null,
            null,
            "sha256=deadbeef",
            payload,
            new ProviderSecretSet(SampleAccessValue, SampleSigningValue, SampleVerifyValue)));

        result.IsValid.Should().BeFalse();
        result.Code.Should().Be("invalid_signature");
    }

    [Fact]
    public void WhatsApp_Signature_Should_Succeed_When_Valid()
    {
        var adapter = CreateWhatsAppAdapter();
        var payload = "{\"object\":\"whatsapp_business_account\"}";
        var secret = SampleSigningValue;
        var signature = ComputeMetaSignature(secret, payload);
        var result = adapter.VerifyPayload(new ProviderWebhookVerificationInput(
            "whatsapp",
            null,
            null,
            null,
            $"sha256={signature}",
            payload,
            new ProviderSecretSet(SampleAccessValue, secret, SampleVerifyValue)));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void WhatsApp_Normalizer_Should_Map_Text_Message()
    {
        var adapter = CreateWhatsAppAdapter();
        var payload = """
                      {
                        "object":"whatsapp_business_account",
                        "entry":[{"id":"entry1","changes":[{"field":"messages","value":{"metadata":{"phone_number_id":"phone123"},"contacts":[{"profile":{"name":"Alice"}}],"messages":[{"id":"wamid.abc","from":"905551112233","timestamp":"1714540800","text":{"body":"hello"}}]}}]}]
                      }
                      """;

        var result = adapter.Normalize(payload);
        var message = result.Messages.First();

        result.Succeeded.Should().BeTrue();
        result.Messages.Should().HaveCount(1);
        message.ChannelAccountId.Should().Be("phone123");
        message.Text.Should().Be("hello");
    }

    [Fact]
    public void Instagram_Normalizer_Should_Map_Dm_Message()
    {
        var adapter = new InstagramMessagingAdapter();
        var payload = """
                      {
                        "object":"instagram",
                        "entry":[{"id":"entry1","messaging":[{"sender":{"id":"user123"},"recipient":{"id":"igbiz789"},"timestamp":1714540800000,"message":{"mid":"m_1","text":"hello ig"}}]}]
                      }
                      """;

        var result = adapter.Normalize(payload);
        var message = result.Messages.First();

        result.Succeeded.Should().BeTrue();
        result.Messages.Should().HaveCount(1);
        message.ChannelAccountId.Should().Be("igbiz789");
        message.Text.Should().Be("hello ig");
    }

    [Fact]
    public void WhatsApp_Outbound_Should_Return_AdapterNotActive_When_Flag_Off()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var adapter = CreateWhatsAppAdapter(handler: handler, outboundEnabled: false);

        var result = adapter.Send(new ProviderOutboundSendInput(
            "whatsapp",
            "905551112233",
            "hello",
            new ProviderSecretSet(SampleAccessValue, SampleSigningValue, SampleVerifyValue),
            "{\"phoneNumberId\":\"12345\"}"));

        result.Code.Should().Be("adapter_not_active");
        handler.CallCount.Should().Be(0);
    }

    [Fact]
    public void WhatsApp_Outbound_Should_Return_MissingFields_When_Config_Invalid()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var adapter = CreateWhatsAppAdapter(handler: handler, outboundEnabled: true);

        var result = adapter.Send(new ProviderOutboundSendInput(
            "whatsapp",
            "905551112233",
            "hello",
            new ProviderSecretSet("", SampleSigningValue, SampleVerifyValue),
            "{\"phoneNumberId\":\"\"}"));

        result.Code.Should().Be("missing_required_fields");
        handler.CallCount.Should().Be(0);
    }

    [Fact]
    public void WhatsApp_Outbound_Should_Send_When_Config_Valid()
    {
        HttpRequestMessage? captured = null;
        var handler = new RecordingHandler(request =>
        {
            captured = request;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"messages\":[{\"id\":\"wamid.sent1\"}]}")
            };
        });
        var adapter = CreateWhatsAppAdapter(handler: handler, outboundEnabled: true);

        var result = adapter.Send(new ProviderOutboundSendInput(
            "whatsapp",
            "905551112233",
            "hello world",
            new ProviderSecretSet(SampleAccessValue, SampleSigningValue, SampleVerifyValue),
            "{\"phoneNumberId\":\"12345\",\"graphApiVersion\":\"v23.0\",\"graphApiBaseUrl\":\"https://graph.facebook.com\"}"));

        result.Succeeded.Should().BeTrue();
        result.ExternalMessageId.Should().Be("wamid.sent1");
        handler.CallCount.Should().Be(1);
        captured.Should().NotBeNull();
        captured!.RequestUri!.AbsoluteUri.Should().Contain("/v23.0/12345/messages");
        captured.Headers.Authorization!.Scheme.Should().Be("Bearer");
    }

    [Fact]
    public void WhatsApp_Outbound_Should_Return_Permanent_On_4xx()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest));
        var adapter = CreateWhatsAppAdapter(handler: handler, outboundEnabled: true);

        var result = adapter.Send(new ProviderOutboundSendInput(
            "whatsapp",
            "905551112233",
            "hello world",
            new ProviderSecretSet(SampleAccessValue, SampleSigningValue, SampleVerifyValue),
            "{\"phoneNumberId\":\"12345\"}"));

        result.Code.Should().Be("provider_permanent_failure");
    }

    [Fact]
    public void WhatsApp_Outbound_Should_Return_Transient_On_5xx()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        var adapter = CreateWhatsAppAdapter(handler: handler, outboundEnabled: true);

        var result = adapter.Send(new ProviderOutboundSendInput(
            "whatsapp",
            "905551112233",
            "hello world",
            new ProviderSecretSet(SampleAccessValue, SampleSigningValue, SampleVerifyValue),
            "{\"phoneNumberId\":\"12345\"}"));

        result.Code.Should().Be("provider_transient_failure");
    }

    [Fact]
    public void WhatsApp_Normalizer_Should_Handle_Unsupported_Payload_Safely()
    {
        var adapter = CreateWhatsAppAdapter();
        var payload = "{\"object\":\"whatsapp_business_account\",\"entry\":[]}";

        var result = adapter.Normalize(payload);
        result.Succeeded.Should().BeFalse();
        result.Code.Should().Be("no_messages");
    }

    [Fact]
    public void NonMock_TestConnection_Should_Require_VerifyToken_Config()
    {
        var catalog = new DefaultIntegrationProviderCatalog(
            new TestHostEnvironment("Development"),
            Options.Create(new IntegrationProviderCatalogOptions { MockProviderEnabled = true }));
        var validator = new DefaultProviderCredentialValidator(catalog);
        var tester = new DefaultProviderConnectionTester(catalog, validator, Options.Create(new WhatsAppProviderOptions()));
        var missing = tester.Test(new ProviderValidationInput("whatsapp", "wa", SampleAccessValue, SampleSigningValue, JsonSerializer.Serialize(new { businessAccountId = "1" })));
        var configured = tester.Test(new ProviderValidationInput("whatsapp", "wa", SampleAccessValue, SampleSigningValue, JsonSerializer.Serialize(new { verifyToken = SampleVerifyValue, businessAccountId = "1" })));

        missing.Code.Should().Be("missing_required_fields");
        configured.Code.Should().Be("missing_required_fields");
    }

    private static string ComputeMetaSignature(string secret, string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
    }

    private static WhatsAppCloudAdapter CreateWhatsAppAdapter(
        HttpMessageHandler? handler = null,
        bool outboundEnabled = false)
    {
        var httpClient = new HttpClient(handler ?? new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)));
        var options = Options.Create(new WhatsAppProviderOptions
        {
            OutboundEnabled = outboundEnabled,
            GraphApiBaseUrl = "https://graph.facebook.com",
            GraphApiVersion = "v23.0",
            TimeoutSeconds = 10
        });

        return new WhatsAppCloudAdapter(httpClient, options);
    }

    private sealed class TestHostEnvironment(string environmentName) : Microsoft.Extensions.Hosting.IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "NetMetric.CRM.IntegrationHub.UnitTests";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.NullFileProvider();
    }

    private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler = handler;
        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(_handler(request));
        }
    }
}
