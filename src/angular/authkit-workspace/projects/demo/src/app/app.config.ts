import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { DEFAULT_AUTH_THEME, provideAuthKitSupabase } from 'authkit';
import { demoConfig } from './config';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideAuthKitSupabase(
      {
        url: demoConfig.url,
        anonKey: demoConfig.anonKey,
        redirectTo: window.location.origin,
      },
      {
        theme: {
          ...DEFAULT_AUTH_THEME,
          primary: '#0f766e',
          radius: '10px',
        },
      },
    ),
  ],
};
