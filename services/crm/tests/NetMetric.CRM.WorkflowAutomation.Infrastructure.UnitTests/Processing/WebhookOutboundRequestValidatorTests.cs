// <copyright file="WebhookOutboundRequestValidatorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.WebhookSubscriptions;
using NetMetric.CRM.WorkflowAutomation.Infrastructure.Processing;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.UnitTests.Processing;

public sealed class WebhookOutboundRequestValidatorTests
{
    [Theory]
    [InlineData("https://127.0.0.1/webhook")]
    [InlineData("https://[::1]/webhook")]
    [InlineData("https://[::ffff:127.0.0.1]/webhook")]
    [InlineData("http://169.254.169.254/latest/meta-data")]
    [InlineData("https://[fd00::1]/webhook")]
    public async Task ValidateAsync_Should_Reject_Blocked_Literal_Address(string targetUrl)
    {
        var validator = CreateValidator();

        var act = () => validator.ValidateAsync(targetUrl, CancellationToken.None);

        await act.Should()
            .ThrowAsync<WorkflowPermanentException>()
            .Where(exception => exception.ErrorCode == "webhook_target_network_not_allowed");
    }

    [Theory]
    [InlineData("https://localhost/webhook")]
    [InlineData("https://foo.localhost/webhook")]
    public async Task ValidateAsync_Should_Reject_Localhost_Names(string targetUrl)
    {
        var validator = CreateValidator();

        var act = () => validator.ValidateAsync(targetUrl, CancellationToken.None);

        await act.Should()
            .ThrowAsync<WorkflowPermanentException>()
            .Where(exception => exception.ErrorCode == "webhook_target_host_invalid");
    }

    [Theory]
    [InlineData("file:///etc/passwd")]
    [InlineData("ftp://example.com/webhook")]
    [InlineData("gopher://example.com/webhook")]
    public async Task ValidateAsync_Should_Reject_Disallowed_Schemes(string targetUrl)
    {
        var validator = CreateValidator();

        var act = () => validator.ValidateAsync(targetUrl, CancellationToken.None);

        await act.Should()
            .ThrowAsync<WorkflowPermanentException>()
            .Where(exception => exception.ErrorCode == "webhook_target_scheme_not_allowed");
    }

    [Fact]
    public async Task ValidateAsync_Should_Reject_UserInfo()
    {
        var validator = CreateValidator();

        var act = () => validator.ValidateAsync("https://user:pass@example.com/webhook", CancellationToken.None);

        await act.Should()
            .ThrowAsync<WorkflowPermanentException>()
            .Where(exception => exception.ErrorCode == "webhook_target_userinfo_not_allowed");
    }

    [Fact]
    public async Task ValidateAsync_Should_Reject_Host_Not_In_Allowlist()
    {
        var validator = CreateValidator(options: new WorkflowAutomationOptions
        {
            WebhookAllowedHosts = ["hooks.netmetric.net"]
        });

        var act = () => validator.ValidateAsync("https://example.com/webhook", CancellationToken.None);

        await act.Should()
            .ThrowAsync<WorkflowPermanentException>()
            .Where(exception => exception.ErrorCode == "webhook_target_host_not_allowed");
    }

    [Fact]
    public async Task ValidateAsync_Should_Reject_Private_Dns_Result()
    {
        var validator = CreateValidator(resolver: new FakeDnsResolver(_ => [IPAddress.Parse("10.0.0.5")]));

        var act = () => validator.ValidateAsync("https://hooks.netmetric.net/webhook", CancellationToken.None);

        await act.Should()
            .ThrowAsync<WorkflowPermanentException>()
            .Where(exception => exception.ErrorCode == "webhook_target_network_not_allowed");
    }

    [Fact]
    public async Task ValidateAsync_Should_Reject_When_Any_Dns_Result_Is_Private()
    {
        var validator = CreateValidator(resolver: new FakeDnsResolver(_ => [IPAddress.Parse("93.184.216.34"), IPAddress.Parse("192.168.1.10")]));

        var act = () => validator.ValidateAsync("https://hooks.netmetric.net/webhook", CancellationToken.None);

        await act.Should()
            .ThrowAsync<WorkflowPermanentException>()
            .Where(exception => exception.ErrorCode == "webhook_target_network_not_allowed");
    }

    [Fact]
    public async Task ValidateAsync_Should_Accept_Allowed_Host_And_Trailing_Dot()
    {
        var validator = CreateValidator(
            new WorkflowAutomationOptions { WebhookAllowedHosts = ["example.com"] },
            new FakeDnsResolver(_ => [IPAddress.Parse("93.184.216.34")]));

        var result = await validator.ValidateAsync("https://example.com./webhook?token=secret", CancellationToken.None);

        result.SafeTarget.Should().Be("https://example.com");
        result.ResolvedAddresses.Should().ContainSingle(address => address.Equals(IPAddress.Parse("93.184.216.34")));
    }

    [Fact]
    public async Task ValidateAsync_Should_Normalize_Idn_To_Punycode_For_Allowlist()
    {
        var validator = CreateValidator(
            new WorkflowAutomationOptions { WebhookAllowedHosts = ["xn--bcher-kva.example"] },
            new FakeDnsResolver(_ => [IPAddress.Parse("93.184.216.34")]));

        var result = await validator.ValidateAsync("https://bücher.example/webhook", CancellationToken.None);

        result.SafeTarget.Should().Be("https://xn--bcher-kva.example");
    }

    [Theory]
    [InlineData("https://evil-example.com/webhook")]
    [InlineData("https://example.com/webhook")]
    [InlineData("https://a.b.example.com/webhook")]
    public async Task ValidateAsync_Should_Reject_Wildcard_Allowlist_Bypass_Attempts(string targetUrl)
    {
        var validator = CreateValidator(
            new WorkflowAutomationOptions { WebhookAllowedHosts = ["*.example.com"] },
            new FakeDnsResolver(_ => [IPAddress.Parse("93.184.216.34")]));

        var act = () => validator.ValidateAsync(targetUrl, CancellationToken.None);

        await act.Should()
            .ThrowAsync<WorkflowPermanentException>()
            .Where(exception => exception.ErrorCode == "webhook_target_host_not_allowed");
    }

    [Fact]
    public async Task ValidateAsync_Should_Accept_One_Label_Wildcard_Subdomain()
    {
        var validator = CreateValidator(
            new WorkflowAutomationOptions { WebhookAllowedHosts = ["*.example.com"] },
            new FakeDnsResolver(_ => [IPAddress.Parse("93.184.216.34")]));

        var result = await validator.ValidateAsync("https://hooks.example.com/webhook", CancellationToken.None);

        result.SafeTarget.Should().Be("https://hooks.example.com");
    }

    [Fact]
    public void CreateSafeAuditTarget_Should_Remove_Query_Fragment_UserInfo_And_Path()
    {
        var sanitized = WebhookOutboundRequestValidator.CreateSafeAuditTarget("https://user:secret@hooks.netmetric.net:8443/webhook/token?token=secret#fragment");

        sanitized.Should().Be("https://hooks.netmetric.net:8443");
    }

    [Fact]
    public async Task ResponsePreviewReader_Should_Bound_Response_Body_And_Mark_Truncated()
    {
        using var content = new StringContent(new string('a', 5000), Encoding.UTF8, "text/plain");

        var preview = await WebhookResponsePreviewReader.ReadAsync(content, 1024, CancellationToken.None);

        preview.Should().EndWith(" [truncated]");
        preview.Length.Should().Be(1024 + " [truncated]".Length);
    }

    [Fact]
    public void SignatureAuditMetadata_Should_Not_Store_Raw_Signature()
    {
        var metadata = WebhookSignatureAuditMetadata.Create("t=1,v1=raw-secret-signature");

        metadata.Should().Be("signature=present;algorithm=hmac-sha256");
        metadata.Should().NotContain("raw-secret-signature");
    }

    [Fact]
    public void CreateBlockedAttempt_Should_Record_Failed_NonRetryable_Audit()
    {
        var delivery = WebhookDeliveryLog.CreateBlockedAttempt(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "customer.created",
            "https://localhost",
            "Blocked by outbound policy: webhook_target_host_invalid",
            "correlation-id");

        delivery.Status.Should().Be(WebhookDeliveryStatuses.Failed);
        delivery.NextAttemptAtUtc.Should().BeNull();
        delivery.AttemptNumber.Should().Be(1);
        delivery.SignatureHeader.Should().Be("signature=not-generated");
    }

    [Fact]
    public void WebhookHttpClientHandlerFactory_Should_Disable_Redirects_And_Implicit_Proxy()
    {
        using var handler = WebhookHttpClientHandlerFactory.Create();

        handler.AllowAutoRedirect.Should().BeFalse();
        handler.UseProxy.Should().BeFalse();
        handler.PooledConnectionLifetime.Should().Be(TimeSpan.Zero);
        handler.PooledConnectionIdleTimeout.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void WorkflowAutomationOptions_Should_Enable_Strict_Webhook_Connection_Pinning_By_Default()
    {
        var options = new WorkflowAutomationOptions();

        options.StrictWebhookConnectionPinning.Should().BeTrue();
    }

    [Fact]
    public void ApplyRequestConnectionPolicy_Should_Force_Http11_And_ConnectionClose_In_Strict_Mode()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "http://example.test/webhook");

        WebhookHttpClientHandlerFactory.ApplyRequestConnectionPolicy(request, strictConnectionPinning: true);

        request.Version.Should().Be(HttpVersion.Version11);
        request.VersionPolicy.Should().Be(HttpVersionPolicy.RequestVersionExact);
        request.Headers.ConnectionClose.Should().BeTrue();
    }

    [Fact]
    public void ApplyRequestConnectionPolicy_Should_Not_Force_ConnectionClose_In_NonStrict_Mode()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "http://example.test/webhook");

        WebhookHttpClientHandlerFactory.ApplyRequestConnectionPolicy(request, strictConnectionPinning: false);

        request.Headers.ConnectionClose.Should().NotBeTrue();
    }

    [Fact]
    public async Task StrictConnectionPinning_Should_Open_New_Connection_For_Each_Request_And_Isolate_Pinned_State()
    {
        await using var server = LoopbackHttpServer.Start(expectedRequestCount: 2);
        var observedPins = new ConcurrentQueue<IPAddress>();
        var connectCalls = 0;
        using var handler = WebhookHttpClientHandlerFactory.Create(
            strictConnectionPinning: true,
            async (context, cancellationToken) =>
            {
                Interlocked.Increment(ref connectCalls);
                if (!context.InitialRequestMessage.Options.TryGetValue(
                    WebhookHttpClientHandlerFactory.ValidatedAddressesOption,
                    out var addresses) ||
                    addresses.Length == 0)
                {
                    throw new InvalidOperationException("Pinned addresses were not attached to the request.");
                }

                observedPins.Enqueue(addresses[0]);
                return await ConnectToLoopbackServerAsync(server.Port, cancellationToken);
            });
        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };

        await SendPinnedRequestAsync(client, server.Uri, IPAddress.Parse("203.0.113.10"), strictConnectionPinning: true);
        await SendPinnedRequestAsync(client, server.Uri, IPAddress.Parse("203.0.113.11"), strictConnectionPinning: true);
        await server.Completion.WaitAsync(TimeSpan.FromSeconds(5));

        connectCalls.Should().Be(2);
        server.ConnectionCount.Should().Be(2);
        observedPins.Should().Equal(IPAddress.Parse("203.0.113.10"), IPAddress.Parse("203.0.113.11"));
        server.ReceivedRequests.Should().OnlyContain(request => request.Contains("Connection: close", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task NonStrictConnectionPinning_Should_Allow_Connection_Reuse_For_Same_Authority()
    {
        await using var server = LoopbackHttpServer.Start(expectedRequestCount: 2);
        var connectCalls = 0;
        using var handler = WebhookHttpClientHandlerFactory.Create(
            strictConnectionPinning: false,
            async (_, cancellationToken) =>
            {
                Interlocked.Increment(ref connectCalls);
                return await ConnectToLoopbackServerAsync(server.Port, cancellationToken);
            });
        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };

        await SendPinnedRequestAsync(client, server.Uri, IPAddress.Parse("203.0.113.10"), strictConnectionPinning: false);
        await SendPinnedRequestAsync(client, server.Uri, IPAddress.Parse("203.0.113.11"), strictConnectionPinning: false);
        await server.Completion.WaitAsync(TimeSpan.FromSeconds(5));

        connectCalls.Should().Be(1);
        server.ConnectionCount.Should().Be(1);
        server.ReceivedRequests.Should().NotContain(request => request.Contains("Connection: close", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void OptionsValidation_Should_Fail_In_Production_When_Allowlist_Is_Missing()
    {
        var validator = new WorkflowAutomationOptionsValidation(new TestHostEnvironment(Environments.Production));

        var result = validator.Validate(null, new WorkflowAutomationOptions { AllowHttpWebhookTargets = false });

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(message => message.Contains("WebhookAllowedHosts", StringComparison.Ordinal));
    }

    [Fact]
    public void OptionsValidation_Should_Fail_In_Production_When_Http_Targets_Are_Allowed()
    {
        var validator = new WorkflowAutomationOptionsValidation(new TestHostEnvironment(Environments.Production));

        var result = validator.Validate(
            null,
            new WorkflowAutomationOptions
            {
                AllowHttpWebhookTargets = true,
                WebhookAllowedHosts = ["hooks.netmetric.net"]
            });

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(message => message.Contains("AllowHttpWebhookTargets", StringComparison.Ordinal));
    }

    [Fact]
    public void OptionsValidation_Should_Fail_When_Proxy_Is_Enabled()
    {
        var validator = new WorkflowAutomationOptionsValidation(new TestHostEnvironment(Environments.Production));

        var result = validator.Validate(
            null,
            new WorkflowAutomationOptions
            {
                AllowHttpWebhookTargets = false,
                UseWebhookProxy = true,
                WebhookAllowedHosts = ["hooks.netmetric.net"]
            });

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(message => message.Contains("UseWebhookProxy", StringComparison.Ordinal));
    }

    private static WebhookOutboundRequestValidator CreateValidator(
        WorkflowAutomationOptions? options = null,
        IWebhookDnsResolver? resolver = null) =>
        new(
            Options.Create(options ?? new WorkflowAutomationOptions()),
            resolver ?? new FakeDnsResolver(_ => [IPAddress.Parse("93.184.216.34")]));

    private sealed class FakeDnsResolver(Func<Uri, IPAddress[]> resolve) : IWebhookDnsResolver
    {
        public Task<IPAddress[]> ResolveAsync(Uri uri, CancellationToken cancellationToken) => Task.FromResult(resolve(uri));
    }

    private static async Task<Stream> ConnectToLoopbackServerAsync(int port, CancellationToken cancellationToken)
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true
        };

        try
        {
            await socket.ConnectAsync(new IPEndPoint(IPAddress.Loopback, port), cancellationToken);
            return new NetworkStream(socket, ownsSocket: true);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }

    private static async Task SendPinnedRequestAsync(
        HttpClient client,
        Uri uri,
        IPAddress pinnedAddress,
        bool strictConnectionPinning)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Options.Set(WebhookHttpClientHandlerFactory.ValidatedAddressesOption, [pinnedAddress]);
        WebhookHttpClientHandlerFactory.ApplyRequestConnectionPolicy(request, strictConnectionPinning);

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        (await response.Content.ReadAsStringAsync()).Should().Be("OK");
    }

    private sealed class LoopbackHttpServer : IAsyncDisposable
    {
        private readonly TcpListener listener;
        private readonly int expectedRequestCount;
        private readonly ConcurrentQueue<string> receivedRequests = new();
        private int connectionCount;

        private LoopbackHttpServer(TcpListener listener, int expectedRequestCount)
        {
            this.listener = listener;
            this.expectedRequestCount = expectedRequestCount;
            Completion = RunAsync();
        }

        public int Port => ((IPEndPoint)listener.LocalEndpoint).Port;
        public Uri Uri => new($"http://example.test:{Port}/webhook");
        public int ConnectionCount => Volatile.Read(ref connectionCount);
        public string[] ReceivedRequests => receivedRequests.ToArray();
        public Task Completion { get; }

        public static LoopbackHttpServer Start(int expectedRequestCount)
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            return new LoopbackHttpServer(listener, expectedRequestCount);
        }

        public async ValueTask DisposeAsync()
        {
            listener.Stop();
            try
            {
                await Completion.WaitAsync(TimeSpan.FromSeconds(1));
            }
            catch (Exception exception) when (exception is ObjectDisposedException or SocketException or TimeoutException)
            {
            }
        }

        private async Task RunAsync()
        {
            var handled = 0;
            while (handled < expectedRequestCount)
            {
                using var client = await listener.AcceptTcpClientAsync();
                Interlocked.Increment(ref connectionCount);
                await using var stream = client.GetStream();

                while (handled < expectedRequestCount)
                {
                    var request = await ReadRequestAsync(stream);
                    if (request is null)
                    {
                        break;
                    }

                    receivedRequests.Enqueue(request);
                    handled += 1;
                    var response = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Length: 2\r\nConnection: keep-alive\r\n\r\nOK");
                    await stream.WriteAsync(response);
                    await stream.FlushAsync();
                }
            }
        }

        private static async Task<string?> ReadRequestAsync(NetworkStream stream)
        {
            var data = new List<byte>();
            var buffer = new byte[1];

            while (true)
            {
                var read = await stream.ReadAsync(buffer);
                if (read == 0)
                {
                    return data.Count == 0 ? null : Encoding.ASCII.GetString(data.ToArray());
                }

                data.Add(buffer[0]);
                var count = data.Count;
                if (count >= 4 &&
                    data[count - 4] == '\r' &&
                    data[count - 3] == '\n' &&
                    data[count - 2] == '\r' &&
                    data[count - 1] == '\n')
                {
                    return Encoding.ASCII.GetString(data.ToArray());
                }
            }
        }
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "NetMetric.CRM.WorkflowAutomation.Infrastructure.UnitTests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
