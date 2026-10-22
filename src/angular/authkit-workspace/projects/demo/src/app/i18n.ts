import { ENGLISH_AUTH_MESSAGES, type AuthMessages, loadAuthMessages } from 'authkit';

/**
 * Loads a JSON bundle for the browser language, falling back to English.
 * The bundles come from samples/messages, copied into the app assets at build time.
 */
export async function loadDemoMessages(locale: string): Promise<AuthMessages> {
  const language = locale.split('-')[0]?.toLowerCase();

  if (language !== 'pt' && language !== 'es') {
    return ENGLISH_AUTH_MESSAGES;
  }

  try {
    return await loadAuthMessages(`/assets/authkit.${language === 'pt' ? 'pt-BR' : 'es'}.json`);
  } catch {
    // A missing bundle must never break sign-in.
    return ENGLISH_AUTH_MESSAGES;
  }
}
