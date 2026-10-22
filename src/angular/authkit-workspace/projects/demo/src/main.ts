import { bootstrapApplication } from '@angular/platform-browser';
import { App } from './app/app';
import { createAppConfig } from './app/app.config';
import { loadDemoMessages } from './app/i18n';

// The catalog is loaded before bootstrap so every component renders translated on the first paint.
loadDemoMessages(navigator.language).then((messages) =>
  bootstrapApplication(App, createAppConfig(messages)).catch((error) => console.error(error)),
);
