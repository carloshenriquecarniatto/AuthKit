import type { AuthMessageKey } from './auth.models';

const MESSAGES: Record<AuthMessageKey, string> = {
  signIn: 'Sign In',
  signUp: 'Sign Up',
  email: 'Enter your email',
  password: 'Enter your password',
  confirmPassword: 'Confirm Password',
  forgotPassword: 'Forgot your password?',
  dontHaveAccount: "Don't have an account? Sign up",
  haveAccount: 'Already have an account? Sign in',
  sendResetEmail: 'Send password reset email',
  resetEmailSent: 'Password reset email has been sent',
  backToSignIn: 'Back to sign in',
  magicLink: 'Continue with magic link',
  checkYourEmail: 'Check your email inbox!',
  signingIn: 'Signing you in',
  continueWithProvider: 'Continue with {provider}',
  requiredField: 'This field is required',
  invalidEmail: 'Please enter a valid email address',
  passwordTooShort: 'Please enter a password that is at least {min} characters long',
  passwordMismatch: 'Passwords do not match',
  invalidOtp: 'Invalid OTP code',
  enterOtp: 'Enter the code sent',
  verify: 'Verify',
  newPassword: 'Enter new password',
  updatePassword: 'Update Password',
  passwordUpdated: 'Password successfully updated',
  showPassword: 'Show',
  hidePassword: 'Hide',
  unexpectedError: 'An unexpected error occurred',
  sessionExpired: 'Your session has expired. Please sign in again.',
};

/** Resolves user-visible strings. Replace to localise or reword the UI. */
export interface AuthMessages {
  get(key: AuthMessageKey, provider?: string): string;
}

export const ENGLISH_AUTH_MESSAGES: AuthMessages = {
  get(key, provider) {
    const text = MESSAGES[key] ?? key;
    return provider ? text.replace('{provider}', provider) : text;
  },
};

/** Interpolates placeholders used by validation messages. */
export function messageWith(text: string, values: Readonly<Record<string, string>>): string {
  return Object.entries(values).reduce(
    (result, [key, value]) => result.replace(`{${key}}`, value),
    text,
  );
}
