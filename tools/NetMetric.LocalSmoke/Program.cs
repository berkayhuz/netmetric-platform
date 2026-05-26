// <copyright file="Program.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

var options = SmokeOptions.Parse(args);
if (options.SelfTestRedactor)
{
    return RedactorSelfTest.Run();
}
var runner = new SmokeRunner(options);
var exitCode = await runner.RunAsync();
return exitCode;

internal sealed class SmokeRunner(SmokeOptions options)
{
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http = new(new SocketsHttpHandler
    {
        AllowAutoRedirect = false,
        UseCookies = false
    })
    {
        BaseAddress = options.GatewayBaseUri,
        Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds)
    };
    private readonly List<FlowResult> _results = [];
    private readonly Dictionary<string, string> _cookies = new(StringComparer.Ordinal);
    private readonly string _runId = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{RandomNumberGenerator.GetHexString(8).ToLowerInvariant()}";

    public async Task<int> RunAsync()
    {
        Console.WriteLine($"NetMetric local smoke matrix started gateway={options.GatewayBaseUri} runId={_runId}");

        var reachability = await ProbeGatewayAsync();
        if (!reachability.Reachable)
        {
            var detail = Redactor.Sanitize(reachability.Detail);
            _results.Add(new FlowResult("Gateway reachability", "SKIP", $"smoke-{_runId}-gateway-reachability", detail));
            Console.WriteLine($"SKIP flow=\"Gateway reachability\" correlationId=smoke-{_runId}-gateway-reachability detail=\"{detail}\"");
            PrintSummary();
            return options.CiOptional ? 0 : 1;
        }

        var context = new SmokeContext(
            tenantName: $"Smoke Tenant {_runId}",
            userName: $"smoke-{_runId}",
            email: $"smoke-{_runId}@netmetric.local",
            password: $"Nm!{_runId}Aa",
            newPassword: $"Nm!{_runId}Bb");

        await FlowAsync("Register", async correlationId =>
        {
            var response = await SendJsonAsync(HttpMethod.Post, "/api/auth/register", correlationId, new
            {
                tenantName = context.TenantName,
                userName = context.UserName,
                email = context.Email,
                password = context.Password,
                firstName = "Smoke",
                lastName = "User",
                culture = "en-US"
            });

            var document = await ExpectJsonAsync(response, HttpStatusCode.OK);
            context.TenantId = document.RootElement.GetProperty("tenantId").GetGuid();
            context.UserId = document.RootElement.GetProperty("userId").GetGuid();
            context.SessionId = document.RootElement.GetProperty("sessionId").GetGuid();
            CaptureCookies(response);
            context.AccessToken = RequireCookie("__Secure-netmetric-access");
            context.RefreshToken = RequireCookie("__Secure-netmetric-refresh");
        });

        await FlowAsync("Email confirmation", async correlationId =>
        {
            RequireRegisteredContext(context);
            if (options.NoDbTokenSeed)
            {
                throw new SmokeSkip("email confirmation requires DB token seeding until local email/link extraction is available");
            }

            var token = $"email-confirm-{Guid.NewGuid():N}";
            await SeedVerificationTokenAsync(context, "email-confirmation", token);
            var response = await SendJsonAsync(HttpMethod.Post, "/api/auth/confirm-email", correlationId, new
            {
                tenantId = context.TenantId,
                userId = context.UserId,
                token
            });
            await ExpectStatusAsync(response, HttpStatusCode.NoContent);
        });

        await FlowAsync("Login", async correlationId =>
        {
            RequireRegisteredContext(context);
            var session = await LoginAsync(context, context.Password, correlationId);
            context.SessionId = session.SessionId;
            context.AccessToken = session.AccessToken;
            context.RefreshToken = session.RefreshToken;
        });

        await FlowAsync("Account profile auto-create", async correlationId =>
        {
            RequireAuthenticatedContext(context);
            var response = await SendJsonAsync(HttpMethod.Get, "/api/v1/account/profile", correlationId, body: null, context.AccessToken);
            var document = await ExpectJsonAsync(response, HttpStatusCode.OK);
            RequireGuid(document, "userId", context.UserId);
        });

        await FlowAsync("Preferences auto-create", async correlationId =>
        {
            RequireAuthenticatedContext(context);
            var response = await SendJsonAsync(HttpMethod.Get, "/api/v1/account/preferences", correlationId, body: null, context.AccessToken);
            var document = await ExpectJsonAsync(response, HttpStatusCode.OK);
            RequireProperty(document, "theme");
        });

        await FlowAsync("Notification seed via preferences update", async correlationId =>
        {
            RequireAuthenticatedContext(context);
            var getResponse = await SendJsonAsync(HttpMethod.Get, "/api/v1/account/preferences", correlationId, body: null, context.AccessToken);
            var current = await ExpectJsonAsync(getResponse, HttpStatusCode.OK);
            var theme = current.RootElement.GetProperty("theme").GetString() ?? "system";
            var updatedTheme = string.Equals(theme, "dark", StringComparison.OrdinalIgnoreCase) ? "light" : "dark";

            var updateResponse = await SendJsonAsync(HttpMethod.Put, "/api/v1/account/preferences", correlationId, new
            {
                theme = updatedTheme,
                language = current.RootElement.GetProperty("language").GetString(),
                timeZone = current.RootElement.GetProperty("timeZone").GetString(),
                dateFormat = current.RootElement.GetProperty("dateFormat").GetString(),
                defaultOrganizationId = current.RootElement.TryGetProperty("defaultOrganizationId", out var defaultOrganizationId) &&
                    defaultOrganizationId.ValueKind is not JsonValueKind.Null
                    ? defaultOrganizationId.GetGuid()
                    : (Guid?)null,
                version = current.RootElement.GetProperty("version").GetString()
            }, context.AccessToken);

            await ExpectJsonAsync(updateResponse, HttpStatusCode.OK);
        });

        await FlowAsync("Session activity sync", async correlationId =>
        {
            RequireAuthenticatedContext(context);
            await Task.Delay(TimeSpan.FromMilliseconds(250));
            var response = await SendJsonAsync(HttpMethod.Get, "/api/v1/account/sessions", correlationId, body: null, context.AccessToken);
            var document = await ExpectJsonAsync(response, HttpStatusCode.OK);
            var items = document.RootElement.GetProperty("items").EnumerateArray().ToArray();
            if (!items.Any(item => item.GetProperty("id").GetGuid() == context.SessionId))
            {
                throw new SmokeFailure("current auth session was not projected into Account sessions");
            }
        });

        await FlowAsync("MFA challenge", async correlationId =>
        {
            RequireAuthenticatedContext(context);
            var setup = await TrySetupMfaAsync(context, correlationId);
            if (setup is null)
            {
                throw new SmokeSkip("MFA setup requires recent re-auth claims; current public/dev login token does not expose a supported re-auth flow");
            }

            context.MfaSharedKey = setup.SharedKey;
            var code = Totp.Generate(setup.SharedKey, DateTimeOffset.UtcNow);
            var confirm = await SendJsonAsync(HttpMethod.Post, "/api/v1/account/security/mfa/confirm", correlationId, new { verificationCode = code }, context.AccessToken);
            await ExpectJsonAsync(confirm, HttpStatusCode.OK);

            var challenge = await SendJsonAsync(HttpMethod.Post, "/api/auth/login", correlationId, new
            {
                tenantId = context.TenantId,
                emailOrUserName = context.Email,
                password = context.Password
            });
            await ExpectStatusAsync(challenge, HttpStatusCode.Unauthorized);
        });

        await FlowAsync("MFA failure", async correlationId =>
        {
            RequireMfaEnabled(context);
            var response = await SendJsonAsync(HttpMethod.Post, "/api/auth/login", correlationId, new
            {
                tenantId = context.TenantId,
                emailOrUserName = context.Email,
                password = context.Password,
                mfaCode = "000000"
            });
            await ExpectStatusAsync(response, HttpStatusCode.Unauthorized);
        });

        await FlowAsync("MFA success", async correlationId =>
        {
            RequireMfaEnabled(context);
            var code = Totp.Generate(context.MfaSharedKey, DateTimeOffset.UtcNow);
            var session = await LoginAsync(context, context.Password, correlationId, code);
            context.SessionId = session.SessionId;
            context.AccessToken = session.AccessToken;
            context.RefreshToken = session.RefreshToken;
        });

        await FlowAsync("Refresh token rotation", async correlationId =>
        {
            RequireAuthenticatedContext(context);
            var oldRefresh = context.RefreshToken;
            var response = await SendJsonAsync(HttpMethod.Post, "/api/auth/refresh", correlationId, new
            {
                tenantId = context.TenantId,
                sessionId = context.SessionId,
                refreshToken = oldRefresh
            });
            var document = await ExpectJsonAsync(response, HttpStatusCode.OK);
            CaptureCookies(response);
            context.SessionId = document.RootElement.GetProperty("sessionId").GetGuid();
            context.AccessToken = RequireCookie("__Secure-netmetric-access");
            context.RefreshToken = RequireCookie("__Secure-netmetric-refresh");
            if (string.Equals(oldRefresh, context.RefreshToken, StringComparison.Ordinal))
            {
                throw new SmokeFailure("refresh token did not rotate");
            }
        });

        await FlowAsync("Password reset", async correlationId =>
        {
            RequireRegisteredContext(context);
            if (options.NoDbTokenSeed)
            {
                throw new SmokeSkip("password reset requires DB token seeding until local email/link extraction is available");
            }

            var request = await SendJsonAsync(HttpMethod.Post, "/api/auth/forgot-password", correlationId, new
            {
                tenantId = context.TenantId,
                email = context.Email
            });
            await ExpectStatusAsync(request, HttpStatusCode.Accepted);

            var token = $"password-reset-{Guid.NewGuid():N}";
            await SeedVerificationTokenAsync(context, "password-reset", token);
            var reset = await SendJsonAsync(HttpMethod.Post, "/api/auth/reset-password", correlationId, new
            {
                tenantId = context.TenantId,
                userId = context.UserId,
                token,
                newPassword = context.NewPassword
            });
            await ExpectStatusAsync(reset, HttpStatusCode.NoContent);
            context.Password = context.NewPassword;

            var code = string.IsNullOrWhiteSpace(context.MfaSharedKey)
                ? null
                : Totp.Generate(context.MfaSharedKey, DateTimeOffset.UtcNow);
            var session = await LoginAsync(context, context.Password, correlationId, code);
            context.SessionId = session.SessionId;
            context.AccessToken = session.AccessToken;
            context.RefreshToken = session.RefreshToken;
        });

        await FlowAsync("Notification list/count/mark-read/delete", async correlationId =>
        {
            RequireAuthenticatedContext(context);
            JsonDocument? document = null;
            for (var attempt = 1; attempt <= 10; attempt++)
            {
                var listAttempt = await SendJsonAsync(HttpMethod.Get, "/api/v1/account/notifications", correlationId, body: null, context.AccessToken);
                var candidate = await ExpectJsonAsync(listAttempt, HttpStatusCode.OK);
                if (candidate.RootElement.TryGetProperty("totalCount", out var totalCountProperty) &&
                    totalCountProperty.GetInt32() > 0)
                {
                    document = candidate;
                    break;
                }

                candidate.Dispose();
                await Task.Delay(TimeSpan.FromMilliseconds(250));
            }

            if (document is null)
            {
                throw new SmokeFailure("notification list did not include any items after deterministic account audit trigger");
            }

            var items = document.RootElement.GetProperty("items").EnumerateArray().ToArray();
            if (items.Length == 0 || document.RootElement.GetProperty("totalCount").GetInt32() == 0)
            {
                throw new SmokeFailure("no account notification/audit items were available");
            }

            var notificationId = items[0].GetProperty("id").GetGuid();
            var markRead = await SendJsonAsync(HttpMethod.Post, $"/api/v1/account/notifications/{notificationId:D}/read", correlationId, body: null, context.AccessToken);
            await ExpectStatusAsync(markRead, HttpStatusCode.NoContent);

            var unread = await SendJsonAsync(HttpMethod.Get, "/api/v1/account/notifications?filter=unread", correlationId, body: null, context.AccessToken);
            await ExpectJsonAsync(unread, HttpStatusCode.OK);

            var delete = await SendJsonAsync(HttpMethod.Delete, $"/api/v1/account/notifications/{notificationId:D}", correlationId, body: null, context.AccessToken);
            await ExpectStatusAsync(delete, HttpStatusCode.NoContent);
        });

        await FlowAsync("Logout", async correlationId =>
        {
            RequireAuthenticatedContext(context);
            var response = await SendJsonAsync(HttpMethod.Post, "/api/auth/logout", correlationId, new
            {
                tenantId = context.TenantId,
                sessionId = context.SessionId,
                refreshToken = context.RefreshToken
            });
            await ExpectStatusAsync(response, HttpStatusCode.NoContent);
        });

        await CleanupAsync(context);
        PrintSummary();

        var failed = _results.Count(result => result.Status == "FAIL");
        if (failed == 0)
        {
            Console.WriteLine("PASS matrix=auth-account-notification");
            return 0;
        }

        Console.Error.WriteLine($"FAIL matrix=auth-account-notification failed={failed}");
        return 1;
    }

    private async Task FlowAsync(string name, Func<string, Task> action)
    {
        var correlationId = $"smoke-{_runId}-{Slug(name)}";
        try
        {
            await action(correlationId);
            _results.Add(new FlowResult(name, "PASS", correlationId, "ok"));
            Console.WriteLine($"PASS flow=\"{name}\" correlationId={correlationId}");
        }
        catch (SmokeSkip ex)
        {
            var detail = Redactor.Sanitize(ex.Message);
            _results.Add(new FlowResult(name, "SKIP", correlationId, detail));
            Console.WriteLine($"SKIP flow=\"{name}\" correlationId={correlationId} detail=\"{detail}\"");
        }
        catch (Exception ex) when (ex is SmokeFailure or HttpRequestException or TaskCanceledException or SqlException or JsonException)
        {
            var detail = Redactor.Sanitize(ex.Message);
            _results.Add(new FlowResult(name, "FAIL", correlationId, detail));
            Console.Error.WriteLine($"FAIL flow=\"{name}\" correlationId={correlationId} detail=\"{detail}\"");
        }
    }

    private async Task<SessionState> LoginAsync(SmokeContext context, string password, string correlationId, string? mfaCode = null)
    {
        var response = await SendJsonAsync(HttpMethod.Post, "/api/auth/login", correlationId, new
        {
            tenantId = context.TenantId,
            emailOrUserName = context.Email,
            password,
            mfaCode
        });
        var document = await ExpectJsonAsync(response, HttpStatusCode.OK);
        CaptureCookies(response);
        return new SessionState(
            document.RootElement.GetProperty("sessionId").GetGuid(),
            RequireCookie("__Secure-netmetric-access"),
            RequireCookie("__Secure-netmetric-refresh"));
    }

    private async Task<MfaSetup?> TrySetupMfaAsync(SmokeContext context, string correlationId)
    {
        var response = await SendJsonAsync(HttpMethod.Post, "/api/v1/account/security/mfa/setup", correlationId, body: null, context.AccessToken);
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            var body = await response.Content.ReadAsStringAsync();
            if (body.Contains("reauth_required", StringComparison.OrdinalIgnoreCase) ||
                body.Contains("Recent authentication", StringComparison.OrdinalIgnoreCase) ||
                body.Contains("stronger recent authentication", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
        }

        var document = await ExpectJsonAsync(response, HttpStatusCode.OK);
        return new MfaSetup(document.RootElement.GetProperty("sharedKey").GetString() ?? throw new SmokeFailure("MFA setup shared key missing"));
    }

    private async Task<HttpResponseMessage> SendJsonAsync(HttpMethod method, string path, string correlationId, object? body, string? bearer = null)
    {
        using var request = new HttpRequestMessage(method, path);
        request.Headers.TryAddWithoutValidation("X-Correlation-Id", correlationId);
        request.Headers.UserAgent.ParseAdd("NetMetric.LocalSmoke/1.0");
        request.Headers.TryAddWithoutValidation("Origin", options.FrontendOrigin);
        if (!string.IsNullOrWhiteSpace(bearer))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
        }
        if (_cookies.Count > 0)
        {
            request.Headers.TryAddWithoutValidation("Cookie", string.Join("; ", _cookies.Select(pair => $"{pair.Key}={pair.Value}")));
        }
        if (body is not null)
        {
            var payload = JsonSerializer.Serialize(body, _json);
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            using var payloadDocument = JsonDocument.Parse(payload);
            if (payloadDocument.RootElement.ValueKind == JsonValueKind.Object &&
                payloadDocument.RootElement.TryGetProperty("tenantId", out var tenantIdProperty) &&
                tenantIdProperty.ValueKind == JsonValueKind.String &&
                Guid.TryParse(tenantIdProperty.GetString(), out var tenantId))
            {
                request.Headers.TryAddWithoutValidation("X-Tenant-Id", tenantId.ToString("D"));
            }
        }

        return await _http.SendAsync(request);
    }

    private async Task<JsonDocument> ExpectJsonAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        await ExpectStatusAsync(response, expected);
        var content = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(content);
    }

    private static async Task ExpectStatusAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        if (response.StatusCode == expected)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        throw new SmokeFailure($"expected {(int)expected} but got {(int)response.StatusCode}; bodyPreview={Redactor.Sanitize(body)}");
    }

    private void CaptureCookies(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var values))
        {
            return;
        }

        foreach (var header in values)
        {
            var pair = header.Split(';', 2)[0];
            var index = pair.IndexOf('=', StringComparison.Ordinal);
            if (index <= 0)
            {
                continue;
            }
            _cookies[pair[..index]] = pair[(index + 1)..];
        }
    }

    private string RequireCookie(string name)
    {
        if (_cookies.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }
        throw new SmokeFailure($"{name} cookie was not issued");
    }

    private async Task SeedVerificationTokenAsync(SmokeContext context, string purpose, string token)
    {
        if (string.IsNullOrWhiteSpace(options.AuthDbConnectionString))
        {
            throw new SmokeSkip($"{purpose} token seeding skipped because auth DB connection was not provided");
        }

        await using var connection = new SqlConnection(options.AuthDbConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO AuthVerificationTokens
                (Id, UserId, Purpose, TokenHash, Target, ExpiresAtUtc, ConsumedAtUtc, ConsumedByIpAddress, Attempts,
                 TenantId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted, DeletedAt, DeletedBy, IsActive)
            VALUES
                (@Id, @UserId, @Purpose, @TokenHash, @Target, DATEADD(minute, 30, SYSUTCDATETIME()), NULL, NULL, 0,
                 @TenantId, SYSUTCDATETIME(), NULL, 'local-smoke', NULL, 0, NULL, NULL, 1);
            """;
        command.Parameters.AddWithValue("@Id", Guid.NewGuid());
        command.Parameters.AddWithValue("@UserId", context.UserId);
        command.Parameters.AddWithValue("@Purpose", purpose);
        command.Parameters.AddWithValue("@TokenHash", Sha256Hex(token));
        command.Parameters.AddWithValue("@Target", context.Email);
        command.Parameters.AddWithValue("@TenantId", context.TenantId);
        await command.ExecuteNonQueryAsync();
    }

    private static void RequireGuid(JsonDocument document, string property, Guid expected)
    {
        if (!document.RootElement.TryGetProperty(property, out var value) || value.GetGuid() != expected)
        {
            throw new SmokeFailure($"{property} mismatch");
        }
    }

    private static void RequireProperty(JsonDocument document, string property)
    {
        if (!document.RootElement.TryGetProperty(property, out _))
        {
            throw new SmokeFailure($"{property} missing");
        }
    }

    private static void RequireMfaEnabled(SmokeContext context)
    {
        if (string.IsNullOrWhiteSpace(context.MfaSharedKey))
        {
            throw new SmokeSkip("MFA login flows skipped because MFA setup is not available through the current public/dev API contract");
        }
    }

    private static void RequireRegisteredContext(SmokeContext context)
    {
        if (context.TenantId == Guid.Empty || context.UserId == Guid.Empty)
        {
            throw new SmokeSkip("register flow did not complete; dependent flow skipped");
        }
    }

    private static void RequireAuthenticatedContext(SmokeContext context)
    {
        RequireRegisteredContext(context);
        if (context.SessionId == Guid.Empty ||
            string.IsNullOrWhiteSpace(context.AccessToken) ||
            string.IsNullOrWhiteSpace(context.RefreshToken))
        {
            throw new SmokeSkip("login flow did not complete; authenticated flow skipped");
        }
    }

    private async Task<(bool Reachable, string Detail)> ProbeGatewayAsync()
    {
        try
        {
            using var response = await _http.GetAsync("health/ready");
            if ((int)response.StatusCode < 500)
            {
                return (true, $"gateway returned {(int)response.StatusCode}");
            }

            return (false, $"gateway readiness returned {(int)response.StatusCode}");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return (false, ex.Message);
        }
    }

    private async Task CleanupAsync(SmokeContext context)
    {
        var correlationId = $"smoke-{_runId}-cleanup";
        Console.WriteLine($"Cleanup started runId={_runId} correlationId={correlationId}");
        var warnings = new List<string>();

        if (context.TenantId == Guid.Empty || context.UserId == Guid.Empty)
        {
            _results.Add(new FlowResult("Cleanup", "SKIP", correlationId, "tenant/user ids were not created"));
            Console.WriteLine($"SKIP flow=\"Cleanup\" correlationId={correlationId} detail=\"tenant/user ids were not created\"");
            return;
        }

        if (!string.IsNullOrWhiteSpace(options.AccountDbConnectionString))
        {
            try
            {
                await CleanupAccountDbAsync(context);
            }
            catch (Exception ex) when (ex is SqlException or InvalidOperationException)
            {
                warnings.Add($"account cleanup: {Redactor.Sanitize(ex.Message)}");
            }
        }
        else
        {
            warnings.Add("account cleanup skipped because account DB connection was not provided");
        }

        if (!string.IsNullOrWhiteSpace(options.AuthDbConnectionString))
        {
            try
            {
                await CleanupAuthDbAsync(context);
            }
            catch (Exception ex) when (ex is SqlException or InvalidOperationException)
            {
                warnings.Add($"auth cleanup: {Redactor.Sanitize(ex.Message)}");
            }
        }
        else
        {
            warnings.Add("auth cleanup skipped because auth DB connection was not provided");
        }

        if (warnings.Count > 0)
        {
            var detail = string.Join("; ", warnings);
            _results.Add(new FlowResult("Cleanup", "WARN", correlationId, detail));
            Console.Error.WriteLine($"WARN flow=\"Cleanup\" correlationId={correlationId} detail=\"{detail}\"");
            return;
        }

        _results.Add(new FlowResult("Cleanup", "PASS", correlationId, "ok"));
        Console.WriteLine($"PASS flow=\"Cleanup\" correlationId={correlationId}");
    }

    private async Task CleanupAccountDbAsync(SmokeContext context)
    {
        await using var connection = new SqlConnection(options.AccountDbConnectionString);
        await connection.OpenAsync();
        await ExecuteCleanupAsync(connection, """
            DELETE FROM account_user_notification_states WHERE TenantId = @TenantId AND UserId = @UserId;
            DELETE FROM account_user_sessions WHERE TenantId = @TenantId AND UserId = @UserId;
            DELETE FROM account_user_preferences WHERE TenantId = @TenantId AND UserId = @UserId;
            DELETE FROM account_notification_preferences WHERE TenantId = @TenantId AND UserId = @UserId;
            DELETE FROM account_user_profiles WHERE TenantId = @TenantId AND UserId = @UserId;
            DELETE FROM account_trusted_devices WHERE TenantId = @TenantId AND UserId = @UserId;
            DELETE FROM account_user_consents WHERE TenantId = @TenantId AND UserId = @UserId;
            DELETE FROM account_audit_entries WHERE TenantId = @TenantId AND UserId = @UserId;
            """, context);
    }

    private async Task CleanupAuthDbAsync(SmokeContext context)
    {
        await using var connection = new SqlConnection(options.AuthDbConnectionString);
        await connection.OpenAsync();
        await ExecuteCleanupAsync(connection, """
            DELETE FROM UserMfaRecoveryCodes WHERE TenantId = @TenantId AND UserId = @UserId;
            DELETE FROM TrustedDevices WHERE TenantId = @TenantId AND UserId = @UserId;
            DELETE FROM AuthVerificationTokens WHERE TenantId = @TenantId AND UserId = @UserId;
            DELETE FROM UserSessions WHERE TenantId = @TenantId AND UserId = @UserId;
            DELETE FROM AuthAuditEvents WHERE TenantId = @TenantId;
            DELETE FROM UserTenantMemberships WHERE TenantId = @TenantId AND UserId = @UserId;
            DELETE FROM TenantInvitations WHERE TenantId = @TenantId;
            DELETE FROM Users WHERE TenantId = @TenantId AND Id = @UserId AND NormalizedEmail LIKE @EmailPrefix;
            DELETE FROM Tenants WHERE Id = @TenantId AND Name LIKE @TenantNamePrefix;
            """, context);
    }

    private static async Task ExecuteCleanupAsync(SqlConnection connection, string sql, SmokeContext context)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@TenantId", context.TenantId);
        command.Parameters.AddWithValue("@UserId", context.UserId);
        command.Parameters.AddWithValue("@EmailPrefix", "SMOKE-%");
        command.Parameters.AddWithValue("@TenantNamePrefix", "Smoke Tenant %");
        await command.ExecuteNonQueryAsync();
    }

    private void PrintSummary()
    {
        Console.WriteLine();
        Console.WriteLine("Smoke summary:");
        foreach (var result in _results)
        {
            Console.WriteLine($"{result.Status} flow=\"{result.Flow}\" correlationId={result.CorrelationId} detail=\"{result.Detail}\"");
        }
    }

    private static string Sha256Hex(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static string Slug(string value) => new(value.ToLowerInvariant().Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray());
}

internal sealed record SmokeOptions(
    Uri GatewayBaseUri,
    string FrontendOrigin,
    string? AuthDbConnectionString,
    string? AccountDbConnectionString,
    bool NoDbTokenSeed,
    bool CiOptional,
    int TimeoutSeconds,
    bool SelfTestRedactor)
{
    public static SmokeOptions Parse(string[] args)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (!arg.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }
            if (index + 1 < args.Length && !args[index + 1].StartsWith("--", StringComparison.Ordinal))
            {
                values[arg] = args[++index];
            }
            else
            {
                flags.Add(arg);
            }
        }

        var gateway = values.GetValueOrDefault("--gateway") ?? Environment.GetEnvironmentVariable("NETMETRIC_SMOKE_GATEWAY") ?? "http://localhost:5030";
        var frontendOrigin = values.GetValueOrDefault("--frontend-origin") ?? Environment.GetEnvironmentVariable("NETMETRIC_SMOKE_FRONTEND_ORIGIN") ?? "http://localhost:7002";
        var authDb = values.GetValueOrDefault("--auth-db") ?? Environment.GetEnvironmentVariable("NETMETRIC_SMOKE_AUTH_DB");
        var accountDb = values.GetValueOrDefault("--account-db") ?? Environment.GetEnvironmentVariable("NETMETRIC_SMOKE_ACCOUNT_DB");
        var timeout = int.TryParse(values.GetValueOrDefault("--timeout-seconds"), out var parsedTimeout) ? parsedTimeout : 30;
        var noDbTokenSeed = flags.Contains("--no-db-token-seed") || string.IsNullOrWhiteSpace(authDb);
        return new SmokeOptions(
            new Uri(gateway.TrimEnd('/') + "/"),
            frontendOrigin,
            authDb,
            accountDb,
            noDbTokenSeed,
            flags.Contains("--ci-optional"),
            timeout,
            flags.Contains("--self-test-redactor"));
    }
}

internal static class Redactor
{
    private static readonly Regex JwtPattern = new(
        @"\b[A-Za-z0-9_-]{20,}\.[A-Za-z0-9_-]{20,}\.[A-Za-z0-9_-]{20,}\b",
        RegexOptions.Compiled);

    private static readonly Regex SecretJsonPattern = new(
        "(?i)(\"(?:password|newPassword|confirmPassword|token|accessToken|refreshToken|idToken|recoveryCode|mfaCode|verificationCode)\"\\s*:\\s*\")([^\"]*)(\")",
        RegexOptions.Compiled);

    private static readonly Regex HeaderSecretPattern = new(
        "(?im)\\b(Authorization|Cookie|Set-Cookie)\\s*[:=]\\s*[^\\r\\n;]+(?:;[^\\r\\n]*)?",
        RegexOptions.Compiled);

    private static readonly Regex NamedSecretPattern = new(
        "(?i)\\b(password|newPassword|confirmPassword|token|accessToken|refreshToken|idToken|recoveryCode|mfaCode|verificationCode)=([^\\s;&]+)",
        RegexOptions.Compiled);

    private static readonly Regex ConnectionPasswordPattern = new(
        "(?i)(Password\\s*=\\s*)([^;\\s]+)",
        RegexOptions.Compiled);

    private static readonly Regex EmailPattern = new(
        @"\b[A-Z0-9._%+\-]+@[A-Z0-9.\-]+\.[A-Z]{2,}\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string Sanitize(string? value, int maxLength = 360)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var sanitized = value.ReplaceLineEndings(" ").Trim();
        sanitized = JwtPattern.Replace(sanitized, "[redacted:jwt]");
        sanitized = SecretJsonPattern.Replace(sanitized, "$1[redacted]$3");
        sanitized = HeaderSecretPattern.Replace(sanitized, match => $"{match.Groups[1].Value}: [redacted]");
        sanitized = ConnectionPasswordPattern.Replace(sanitized, "$1[redacted]");
        sanitized = NamedSecretPattern.Replace(sanitized, "$1=[redacted]");
        sanitized = EmailPattern.Replace(sanitized, MaskEmail);
        return sanitized.Length <= maxLength ? sanitized : sanitized[..maxLength];
    }

    private static string MaskEmail(Match match)
    {
        var value = match.Value;
        var at = value.IndexOf('@');
        if (at <= 1)
        {
            return "[redacted-email]";
        }

        var local = value[..at];
        var domain = value[(at + 1)..];
        var prefix = local.Length <= 2 ? local[0].ToString() : local[..2];
        return $"{prefix}***@{domain}";
    }
}

internal static class RedactorSelfTest
{
    public static int Run()
    {
        const string sample =
            "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.aaaaaaaaaaaaaaaaaaaaaaaa.bbbbbbbbbbbbbbbbbbbbbbbb " +
            "Set-Cookie: __Secure-netmetric-refresh=refresh-secret; Path=/api/auth " +
            "{\"password\":\"Secret123!\",\"newPassword\":\"Secret456!\",\"token\":\"email-confirm-raw\",\"mfaCode\":\"123456\",\"email\":\"smoke-test@example.com\"}";

        var redacted = Redactor.Sanitize(sample, maxLength: 1000);
        var forbidden = new[] { "Secret123", "Secret456", "email-confirm-raw", "123456", "smoke-test@example.com", "refresh-secret" };
        var leaked = forbidden.Where(redacted.Contains).ToArray();
        if (leaked.Length > 0 || !redacted.Contains("[redacted]", StringComparison.Ordinal))
        {
            Console.Error.WriteLine($"FAIL redactor self-test leaked={string.Join(",", leaked)} output={redacted}");
            return 1;
        }

        Console.WriteLine("PASS redactor self-test");
        return 0;
    }
}

internal sealed class SmokeContext(string tenantName, string userName, string email, string password, string newPassword)
{
    public string TenantName { get; } = tenantName;
    public string UserName { get; } = userName;
    public string Email { get; } = email;
    public string Password { get; set; } = password;
    public string NewPassword { get; } = newPassword;
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid SessionId { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string MfaSharedKey { get; set; } = string.Empty;
}

internal sealed record SessionState(Guid SessionId, string AccessToken, string RefreshToken);
internal sealed record MfaSetup(string SharedKey);
internal sealed record FlowResult(string Flow, string Status, string CorrelationId, string Detail);
internal sealed class SmokeFailure(string message) : Exception(message);
internal sealed class SmokeSkip(string message) : Exception(message);

internal static class Totp
{
    private static readonly char[] Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();

    public static string Generate(string sharedKey, DateTimeOffset now)
    {
        var key = FromBase32(sharedKey);
        var timeStep = now.ToUnixTimeSeconds() / 30;
        Span<byte> counter = stackalloc byte[8];
        for (var i = 7; i >= 0; i--)
        {
            counter[i] = (byte)(timeStep & 0xFF);
            timeStep >>= 8;
        }

        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(counter.ToArray());
        var offset = hash[^1] & 0x0F;
        var binary =
            ((hash[offset] & 0x7F) << 24) |
            ((hash[offset + 1] & 0xFF) << 16) |
            ((hash[offset + 2] & 0xFF) << 8) |
            (hash[offset + 3] & 0xFF);

        return (binary % 1_000_000).ToString("D6", CultureInfo.InvariantCulture);
    }

    private static byte[] FromBase32(string value)
    {
        var clean = value.Trim().Replace(" ", string.Empty, StringComparison.Ordinal).TrimEnd('=').ToUpperInvariant();
        var bytes = new List<byte>(clean.Length * 5 / 8);
        var bitBuffer = 0;
        var bitCount = 0;
        foreach (var ch in clean)
        {
            var index = Array.IndexOf(Base32Alphabet, ch);
            if (index < 0)
            {
                throw new SmokeFailure("MFA shared key contains invalid base32 characters");
            }

            bitBuffer = (bitBuffer << 5) | index;
            bitCount += 5;
            if (bitCount >= 8)
            {
                bytes.Add((byte)((bitBuffer >> (bitCount - 8)) & 0xFF));
                bitCount -= 8;
            }
        }

        return bytes.ToArray();
    }
}
