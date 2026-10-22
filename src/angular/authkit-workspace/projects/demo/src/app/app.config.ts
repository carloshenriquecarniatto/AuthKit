import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { DEFAULT_AUTH_THEME, provideAuthKitSupabase, type AuthMessages } from 'authkit';
import { demoConfig } from './config';

export function createAppConfig(messages: AuthMessages): ApplicationConfig {
  return {
    providers: [
      provideBrowserGlobalErrorListeners(),
      provideAuthKitSupabase(
        {
          url: demoConfig.url,
          anonKey: demoConfig.anonKey,
          redirectTo: window.location.origin,
        },
        {
          messages,
          theme: {
            ...DEFAULT_AUTH_THEME,
            primary: '#0f766e',
            radius: '10px',
          },
        },
      ),
    ],
  };
}
