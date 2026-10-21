namespace AuthKit;

/// <summary>
/// Stable, provider-independent error taxonomy. Mirrors <c>AuthErrorCode</c> in
/// contracts/authkit.contract.json; the wire representation is snake_case.
/// </summary>
public enum AuthErrorCode
{
    /// <summary>The email/password pair was rejected.</summary>
    InvalidCredentials,

    /// <summary>The account exists but the email was never confirmed.</summary>
    EmailNotConfirmed,

    /// <summary>A sign-up collided with an existing account.</summary>
    UserAlreadyExists,

    /// <summary>The provider rejected the password as too weak.</summary>
    WeakPassword,

    /// <summary>The OTP or link has expired.</summary>
    OtpExpired,

    /// <summary>The OTP is wrong.</summary>
    InvalidOtp,

    /// <summary>The provider is throttling requests.</summary>
    RateLimited,

    /// <summary>The requested social provider is not enabled.</summary>
    ProviderDisabled,

    /// <summary>A second factor is required.</summary>
    MfaRequired,

    /// <summary>The session is gone and must be re-established.</summary>
    SessionExpired,

    /// <summary>The request never reached the provider, or the response was unreadable.</summary>
    NetworkError,

    /// <summary>Anything not covered above. The message is safe to display.</summary>
    Unknown
}

/// <summary>
/// A structured authentication failure. <see cref="Message"/> must already be safe for
/// end users: providers are responsible for mapping raw server text before constructing it.
/// </summary>
public sealed record AuthError
{
    /// <summary>Stable error category.</summary>
    public required AuthErrorCode Code { get; init; }

    /// <summary>Short, user-safe description.</summary>
    public required string Message { get; init; }

    /// <summary>HTTP status returned by the provider, when there was one.</summary>
    public int? StatusCode { get; init; }

    /// <summary>Creates an error with no HTTP status.</summary>
    public static AuthError From(AuthErrorCode code, string message) =>
        new() { Code = code, Message = message };

    /// <summary>Creates an error carrying the provider's HTTP status.</summary>
    public static AuthError From(AuthErrorCode code, string message, int? statusCode) =>
        new() { Code = code, Message = message, StatusCode = statusCode };
}

/// <summary>Wire values for the contract enums. Consumed by the contract parity tests.</summary>
public static class AuthContractValues
{
    /// <summary>snake_case representation of every <see cref="AuthErrorCode"/>.</summary>
    public static IReadOnlyDictionary<AuthErrorCode, string> ErrorCodes { get; } =
        new Dictionary<AuthErrorCode, string>
        {
            [AuthErrorCode.InvalidCredentials] = "invalid_credentials",
            [AuthErrorCode.EmailNotConfirmed] = "email_not_confirmed",
            [AuthErrorCode.UserAlreadyExists] = "user_already_exists",
            [AuthErrorCode.WeakPassword] = "weak_password",
            [AuthErrorCode.OtpExpired] = "otp_expired",
            [AuthErrorCode.InvalidOtp] = "invalid_otp",
            [AuthErrorCode.RateLimited] = "rate_limited",
            [AuthErrorCode.ProviderDisabled] = "provider_disabled",
            [AuthErrorCode.MfaRequired] = "mfa_required",
            [AuthErrorCode.SessionExpired] = "session_expired",
            [AuthErrorCode.NetworkError] = "network_error",
            [AuthErrorCode.Unknown] = "unknown"
        };

    /// <summary>snake_case representation of every <see cref="AuthState"/>.</summary>
    public static IReadOnlyDictionary<AuthState, string> States { get; } =
        new Dictionary<AuthState, string>
        {
            [AuthState.Idle] = "idle",
            [AuthState.Submitting] = "submitting",
            [AuthState.AwaitingEmailConfirmation] = "awaiting_email_confirmation",
            [AuthState.AwaitingOtp] = "awaiting_otp",
            [AuthState.Authenticated] = "authenticated",
            [AuthState.Error] = "error"
        };

    /// <summary>snake_case representation of every <see cref="AuthNextStep"/>.</summary>
    public static IReadOnlyDictionary<AuthNextStep, string> NextSteps { get; } =
        new Dictionary<AuthNextStep, string>
        {
            [AuthNextStep.None] = "none",
            [AuthNextStep.ConfirmEmail] = "confirm_email",
            [AuthNextStep.VerifyOtp] = "verify_otp",
            [AuthNextStep.Mfa] = "mfa"
        };

    /// <summary>snake_case representation of every <see cref="AuthMessageKey"/>.</summary>
    public static IReadOnlyDictionary<AuthMessageKey, string> MessageKeys { get; } =
        new Dictionary<AuthMessageKey, string>
        {
            [AuthMessageKey.SignIn] = "signIn",
            [AuthMessageKey.SignUp] = "signUp",
            [AuthMessageKey.Email] = "email",
            [AuthMessageKey.Password] = "password",
            [AuthMessageKey.ConfirmPassword] = "confirmPassword",
            [AuthMessageKey.ForgotPassword] = "forgotPassword",
            [AuthMessageKey.DontHaveAccount] = "dontHaveAccount",
            [AuthMessageKey.HaveAccount] = "haveAccount",
            [AuthMessageKey.SendResetEmail] = "sendResetEmail",
            [AuthMessageKey.ResetEmailSent] = "resetEmailSent",
            [AuthMessageKey.BackToSignIn] = "backToSignIn",
            [AuthMessageKey.MagicLink] = "magicLink",
            [AuthMessageKey.CheckYourEmail] = "checkYourEmail",
            [AuthMessageKey.SigningIn] = "signingIn",
            [AuthMessageKey.ContinueWithProvider] = "continueWithProvider",
            [AuthMessageKey.RequiredField] = "requiredField",
            [AuthMessageKey.InvalidEmail] = "invalidEmail",
            [AuthMessageKey.PasswordTooShort] = "passwordTooShort",
            [AuthMessageKey.PasswordMismatch] = "passwordMismatch",
            [AuthMessageKey.InvalidOtp] = "invalidOtp",
            [AuthMessageKey.EnterOtp] = "enterOtp",
            [AuthMessageKey.Verify] = "verify",
            [AuthMessageKey.NewPassword] = "newPassword",
            [AuthMessageKey.UpdatePassword] = "updatePassword",
            [AuthMessageKey.PasswordUpdated] = "passwordUpdated",
            [AuthMessageKey.ShowPassword] = "showPassword",
            [AuthMessageKey.HidePassword] = "hidePassword",
            [AuthMessageKey.UnexpectedError] = "unexpectedError",
            [AuthMessageKey.SessionExpired] = "sessionExpired"
        };
}
