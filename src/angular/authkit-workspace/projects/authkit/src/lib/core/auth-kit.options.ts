import { InjectionToken, type Provider, makeEnvironmentProviders } from '@angular/core';
import { DEFAULT_AUTH_THEME, type AuthTheme } from '../contract/auth.models';
import { ENGLISH_AUTH_MESSAGES, type AuthMessages } from '../contract/auth.messages';
import { DEFAULT_MINIMUM_PASSWORD_LENGTH } from '../contract/auth.validation';
import type { AuthProvider } from '../contract/auth.provider';
import { SupabaseAuthProvider, type SupabaseAuthOptions } from '../supabase/supabase-auth.provider';

/** UI settings shared by every AuthKit component. */
export interface AuthKitOptions {
  readonly theme: AuthTheme;
  readonly messages: AuthMessages;
  readonly redirectTo: string | null;
  readonly minimumPasswordLength: number;
  readonly socialProviders: readonly string[];
}

export const AUTH_PROVIDER = new InjectionToken<AuthProvider>('AUTHKIT_PROVIDER');

export const AUTHKIT_OPTIONS = new InjectionToken<AuthKitOptions>('AUTHKIT_OPTIONS', {
  factory: () => ({
    theme: DEFAULT_AUTH_THEME,
    messages: ENGLISH_AUTH_MESSAGES,
    redirectTo: null,
    minimumPasswordLength: DEFAULT_MINIMUM_PASSWORD_LENGTH,
    socialProviders: [],
  }),
});

export interface AuthKitUiOptions {
  readonly theme?: AuthTheme;
  readonly messages?: AuthMessages;
  readonly redirectTo?: string;
  readonly minimumPasswordLength?: number;
  readonly socialProviders?: readonly string[];
}

/** Registers Supabase Auth as the AuthKit provider. */
export function provideAuthKitSupabase(
  supabase: SupabaseAuthOptions,
  ui: AuthKitUiOptions = {},
): ReturnType<typeof makeEnvironmentProviders> {
  return makeEnvironmentProviders([
    {
      provide: AUTH_PROVIDER,
      useFactory: () => new SupabaseAuthProvider(supabase),
    },
    {
      provide: AUTHKIT_OPTIONS,
      useValue: toOptions(ui, supabase.redirectTo, supabase.socialProviders),
    },
  ]);
}

/** Registers any AuthProvider implementation. */
export function provideAuthKit(
  provider: AuthProvider | (() => AuthProvider),
  ui: AuthKitUiOptions = {},
): ReturnType<typeof makeEnvironmentProviders> {
  const providers: Provider[] = [
    typeof provider === 'function'
      ? { provide: AUTH_PROVIDER, useFactory: provider }
      : { provide: AUTH_PROVIDER, useValue: provider },
    { provide: AUTHKIT_OPTIONS, useValue: toOptions(ui) },
  ];

  return makeEnvironmentProviders(providers);
}

function toOptions(
  ui: AuthKitUiOptions,
  redirectTo?: string,
  socialProviders?: readonly string[],
): AuthKitOptions {
  return {
    theme: ui.theme ?? DEFAULT_AUTH_THEME,
    messages: ui.messages ?? ENGLISH_AUTH_MESSAGES,
    redirectTo: ui.redirectTo ?? redirectTo ?? null,
    minimumPasswordLength: ui.minimumPasswordLength ?? DEFAULT_MINIMUM_PASSWORD_LENGTH,
    socialProviders: ui.socialProviders ?? socialProviders ?? [],
  };
}
