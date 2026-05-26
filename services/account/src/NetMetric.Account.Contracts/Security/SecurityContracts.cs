// <copyright file="SecurityContracts.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Contracts.Security;

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmNewPassword);

public sealed record PasswordPolicyFailureResponse(string Code, string Message);

public sealed record MfaStatusResponse(bool IsEnabled, bool HasAuthenticator, int RecoveryCodesRemaining);

public sealed record MfaSetupResponse(string SharedKey, string AuthenticatorUri);

public sealed record ConfirmMfaRequest(string VerificationCode);

public sealed record ConfirmMfaResponse(bool IsEnabled, IReadOnlyCollection<string> RecoveryCodes);

public sealed record DisableMfaRequest(string VerificationCode);

public sealed record RecoveryCodesResponse(IReadOnlyCollection<string> RecoveryCodes);
