# Local Development Guide

## 1. Install dependencies

```bash
pnpm install
```

## 2. Setup environment files

Copy templates from:

- `.env.example`
- `apps/<app-name>/.env.example`

and fill local non-sensitive values.

## 3. Validate workspace

```bash
pnpm run repo:validate
```

## 4. Run baseline checks

```bash
pnpm run frontend:lint
pnpm run frontend:typecheck
pnpm run frontend:build
pnpm run frontend:test
pnpm run frontend:coverage
pnpm run repo:format:check
```

## 5. Optional health checks

```bash
pnpm run frontend:health
```

## 6. Hook behavior

Installed git hooks:

- `pre-commit`: secret scan, lint-staged, selective typecheck
- `commit-msg`: Conventional Commit validation
- `pre-push`: optional local gitleaks + workspace validation + build verification

If hooks are not active, run:

```bash
pnpm run prepare
```

## 7. Auth email smoke with Gmail SMTP

Auth account lifecycle emails are produced as Auth integration events and sent by the Notification worker. Keep Gmail app passwords out of git; store them in user-secrets or environment variables only.

Set local Notification SMTP secrets:

```powershell
dotnet user-secrets set "Notification:Email:Smtp:FromAddress" "<gmail-address>" --project services/notification/src/NetMetric.Notification.Worker
dotnet user-secrets set "Notification:Email:Smtp:UserName" "<gmail-address>" --project services/notification/src/NetMetric.Notification.Worker
dotnet user-secrets set "Notification:Email:Smtp:Password" "<gmail-app-password>" --project services/notification/src/NetMetric.Notification.Worker
```

For direct Auth invitation SMTP testing only, switch the Auth invitation provider locally and set the same SMTP values on Auth:

```powershell
dotnet user-secrets set "InvitationDelivery:Provider" "smtp" --project services/auth/src/NetMetric.Auth.API
dotnet user-secrets set "InvitationDelivery:SenderName" "NetMetricApp" --project services/auth/src/NetMetric.Auth.API
dotnet user-secrets set "InvitationDelivery:SenderAddress" "<gmail-address>" --project services/auth/src/NetMetric.Auth.API
dotnet user-secrets set "InvitationDelivery:SmtpHost" "smtp.gmail.com" --project services/auth/src/NetMetric.Auth.API
dotnet user-secrets set "InvitationDelivery:SmtpPort" "587" --project services/auth/src/NetMetric.Auth.API
dotnet user-secrets set "InvitationDelivery:SmtpUseStartTls" "true" --project services/auth/src/NetMetric.Auth.API
dotnet user-secrets set "InvitationDelivery:SmtpUserName" "<gmail-address>" --project services/auth/src/NetMetric.Auth.API
dotnet user-secrets set "InvitationDelivery:SmtpPassword" "<gmail-app-password>" --project services/auth/src/NetMetric.Auth.API
```

Start local dependencies and Auth, then run the Notification worker:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\dev-up.ps1
dotnet run --project services/notification/src/NetMetric.Notification.Worker
```

Trigger registration and forgot password through Auth:

```powershell
$email = "<gmail-address>"
$register = @{
  tenantName = "Email Smoke"
  userName = "email-smoke"
  email = $email
  password = "[PASSWORD]"
  firstName = "Email"
  lastName = "Smoke"
  culture = "en-US"
} | ConvertTo-Json
$response = Invoke-RestMethod -Method Post -Uri "http://localhost:5297/api/auth/register" -ContentType "application/json" -Body $register
$tenantId = $response.tenantId
$forgot = @{ tenantId = $tenantId; email = $email } | ConvertTo-Json
Invoke-RestMethod -Method Post -Uri "http://localhost:5297/api/auth/forgot-password" -ContentType "application/json" -Body $forgot
```

Confirm the Gmail inbox receives the confirmation and password reset messages. Do not paste tokens, cookies, Authorization headers, or app passwords into logs or issue comments.
