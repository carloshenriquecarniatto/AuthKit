namespace AuthKit;

/// <summary>
/// Every user-visible string a component can render. Mirrors <c>AuthMessageKey</c> in the
/// contract, so the Angular client exposes the same keys.
/// </summary>
public enum AuthMessageKey
{
    SignIn,
    SignUp,
    Email,
    Password,
    ConfirmPassword,
    ForgotPassword,
    DontHaveAccount,
    HaveAccount,
    SendResetEmail,
    ResetEmailSent,
    BackToSignIn,
    MagicLink,
    CheckYourEmail,
    SigningIn,
    ContinueWithProvider,
    RequiredField,
    InvalidEmail,
    PasswordTooShort,
    PasswordMismatch,
    InvalidOtp,
    EnterOtp,
    Verify,
    NewPassword,
    UpdatePassword,
    PasswordUpdated,
    ShowPassword,
    HidePassword,
    UnexpectedError,
    SessionExpired
}

/// <summary>Resolves user-visible strings. Implement to localise or reword the UI.</summary>
public interface IAuthMessages
{
    /// <summary>
    /// Returns the text for a key. <paramref name="provider"/> is supplied only for
    /// <see cref="AuthMessageKey.ContinueWithProvider"/>.
    /// </summary>
    string Resolve(AuthMessageKey key, string? provider = null);
}

/// <summary>English strings, matching the wording used by supabase_auth_ui.</summary>
public sealed class EnglishAuthMessages : IAuthMessages
{
    private static readonly Dictionary<AuthMessageKey, string> Strings =
        new Dictionary<AuthMessageKey, string>
        {
            [AuthMessageKey.SignIn] = "Sign In",
            [AuthMessageKey.SignUp] = "Sign Up",
            [AuthMessageKey.Email] = "Enter your email",
            [AuthMessageKey.Password] = "Enter your password",
            [AuthMessageKey.ConfirmPassword] = "Confirm Password",
            [AuthMessageKey.ForgotPassword] = "Forgot your password?",
            [AuthMessageKey.DontHaveAccount] = "Don't have an account? Sign up",
            [AuthMessageKey.HaveAccount] = "Already have an account? Sign in",
            [AuthMessageKey.SendResetEmail] = "Send password reset email",
            [AuthMessageKey.ResetEmailSent] = "Password reset email has been sent",
            [AuthMessageKey.BackToSignIn] = "Back to sign in",
            [AuthMessageKey.MagicLink] = "Continue with magic link",
            [AuthMessageKey.CheckYourEmail] = "Check your email inbox!",
            [AuthMessageKey.SigningIn] = "Signing you in",
            [AuthMessageKey.ContinueWithProvider] = "Continue with {provider}",
            [AuthMessageKey.RequiredField] = "This field is required",
            [AuthMessageKey.InvalidEmail] = "Please enter a valid email address",
            [AuthMessageKey.PasswordTooShort] = "Please enter a password that is at least {min} characters long",
            [AuthMessageKey.PasswordMismatch] = "Passwords do not match",
            [AuthMessageKey.InvalidOtp] = "Invalid OTP code",
            [AuthMessageKey.EnterOtp] = "Enter the code sent",
            [AuthMessageKey.Verify] = "Verify",
            [AuthMessageKey.NewPassword] = "Enter new password",
            [AuthMessageKey.UpdatePassword] = "Update Password",
            [AuthMessageKey.PasswordUpdated] = "Password successfully updated",
            [AuthMessageKey.ShowPassword] = "Show",
            [AuthMessageKey.HidePassword] = "Hide",
            [AuthMessageKey.UnexpectedError] = "An unexpected error occurred",
            [AuthMessageKey.SessionExpired] = "Your session has expired. Please sign in again."
        };

    /// <inheritdoc />
    public string Resolve(AuthMessageKey key, string? provider = null)
    {
        var text = Strings.TryGetValue(key, out var value) ? value : key.ToString();
        return provider is null
            ? text
            : text.Replace("{provider}", provider, StringComparison.Ordinal);
    }
}

/// <summary>Access to the bundled message sets.</summary>
public static class AuthMessages
{
    /// <summary>English messages.</summary>
    public static IAuthMessages English { get; } = new EnglishAuthMessages();
}
