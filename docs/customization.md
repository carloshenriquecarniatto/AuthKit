---
layout: page
title: Customizing the UI
permalink: /customization/
---

**English** · [Português]({{ site.baseurl }}/personalizacao/) ·
[Español]({{ site.baseurl }}/personalizacion/)

AuthKit gives you a working login screen in one line, but nothing about it is locked. There are
three levels of customization, and you can stop at whichever one solves your problem.

| Level           | You change                           | You keep                                       | Effort  |
| --------------- | ------------------------------------ | ---------------------------------------------- | ------- |
| 1. Theme tokens | Colours, radius, spacing, font, logo | Every component and behaviour                  | Minutes |
| 2. CSS          | Layout, density, shadows, animations | Every component and behaviour                  | Hours   |
| 3. Headless     | The whole UI                         | The provider, storage, error mapping, security | Days    |

![Sign in form]({{ site.baseurl }}/assets/login.png)

## Level 1 — theme tokens

`AuthTheme` is a plain record with ten tokens. Both SDKs use the same names, and on the web they
become CSS custom properties on the component root.

| Token        | CSS variable       | Default         |
| ------------ | ------------------ | --------------- |
| `Primary`    | `--ak-primary`     | `#4f46e5`       |
| `Background` | `--ak-background`  | `#ffffff`       |
| `Surface`    | `--ak-surface`     | `#ffffff`       |
| `Text`       | `--ak-text`        | `#111827`       |
| `TextMuted`  | `--ak-text-muted`  | `#6b7280`       |
| `Border`     | `--ak-border`      | `#d1d5db`       |
| `Danger`     | `--ak-danger`      | `#dc2626`       |
| `Radius`     | `--ak-radius`      | `8px`           |
| `Spacing`    | `--ak-spacing`     | `16px`          |
| `FontFamily` | `--ak-font-family` | system UI stack |

### Blazor

```csharp
builder.Services.AddAuthKitSupabase(
    supabase =>
    {
        supabase.Url = "https://your-project.supabase.co";
        supabase.AnonKey = "<anon key>";
    },
    ui =>
    {
        ui.Theme = AuthTheme.Default with
        {
            Primary = "#0f766e",
            Radius = "12px",
            FontFamily = "Inter, system-ui, sans-serif",
            LogoUrl = "/logo.svg"
        };
    });
```

### Angular

```ts
provideAuthKitSupabase(
  { url: environment.supabaseUrl, anonKey: environment.anonKey },
  {
    theme: {
      ...DEFAULT_AUTH_THEME,
      primary: '#0f766e',
      radius: '12px',
      fontFamily: 'Inter, system-ui, sans-serif',
      logoUrl: '/logo.svg',
    },
  },
);
```

Theme values are sanitised: `; { } < > " ' \` are stripped and `url(`/`expression(` are rejected, so
a theme coming from user input cannot inject CSS. `LogoUrl` only accepts https or a relative path.

## Level 2 — your own CSS

Every class is prefixed with `ak-`. Override them globally, or turn the bundled styles off and write
your own.

```css
/* Loaded after AuthKit's styles, or with IncludeStyles="false" in Blazor. */
.ak-form {
  gap: 20px;
}

.ak-input {
  background: #0b1220;
  color: #e2e8f0;
  border-color: #1e293b;
}

.ak-button {
  text-transform: uppercase;
  letter-spacing: 0.04em;
}
```

Useful hooks: `.ak-form`, `.ak-field`, `.ak-label`, `.ak-input`, `.ak-hint`, `.ak-button`,
`.ak-link`, `.ak-error`, `.ak-notice`, `.ak-socials`, `.ak-social`, `.ak-spinner`.

### Blazor

`<AuthKitEmail IncludeStyles="false" />` stops emitting the base stylesheet; then include your own
CSS in the host page. Every component accepts `IncludeStyles`.

### Angular

The components use `ViewEncapsulation.None`, so a global stylesheet in your app can override the
`ak-*` classes directly.

## Level 3 — headless

When the design is nothing like a login card, stop using the components and use the provider. You
keep the parts that are hard to get right: error mapping, session storage, refresh handling and
validation.

### Blazor

```razor
@inject IAuthProvider Auth
@inject NavigationManager Nav

<form @onsubmit="Submit" @onsubmit:preventDefault class="my-login">
    <input @bind="_email" placeholder="you@company.com" />
    <input type="password" @bind="_password" />
    <button disabled="@_busy">@(_busy ? "…" : "Enter")</button>
</form>

@if (_error is not null)
{
    <p role="alert">@_error</p>
}

@code {
    private string _email = "";
    private string _password = "";
    private bool _busy;
    private string? _error;

    private async Task Submit()
    {
        _busy = true;
        var result = await Auth.SignInWithPasswordAsync(_email, _password);
        _busy = false;

        if (result.Ok)
        {
            Nav.NavigateTo("/");
        }
        else
        {
            // Already user-safe: provider text is mapped by the SDK.
            _error = result.Error?.Message;
        }
    }
}
```

### Angular

```ts
import { Component, inject } from '@angular/core';
import { AUTH_PROVIDER } from '@authkit/angular';

@Component({
  selector: 'app-sign-in',
  template: `
    <form (submit)="submit($event)">
      <input [value]="email" (input)="email = valueOf($event)" />
      <input type="password" [value]="password" (input)="password = valueOf($event)" />
      <button [disabled]="busy">Enter</button>
    </form>
  `,
})
export class SignInComponent {
  private readonly auth = inject(AUTH_PROVIDER);

  email = '';
  password = '';
  busy = false;

  valueOf(event: Event): string {
    return (event.target as HTMLInputElement).value;
  }

  async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.busy = true;
    const result = await this.auth.signInWithPassword(this.email, this.password);
    this.busy = false;
    // result.error?.message is safe to display.
  }
}
```

The provider also exposes `capabilities` (what the project supports), `onStateChange` /
`StateChanged`, and the whole operation surface listed in `contracts/authkit.contract.json`.

## Messages and localization

English ships in the box. Replace the table to translate or reword every built-in string.

```csharp
public sealed class PtBrMessages : IAuthMessages
{
    private static readonly Dictionary<AuthMessageKey, string> Strings = new()
    {
        [AuthMessageKey.SignIn] = "Entrar",
        [AuthMessageKey.Email] = "Seu e-mail",
        [AuthMessageKey.Password] = "Sua senha",
        [AuthMessageKey.InvalidEmail] = "Informe um e-mail válido",
        [AuthMessageKey.PasswordTooShort] =
            "A senha precisa ter pelo menos {min} caracteres",
    };

    public string Resolve(AuthMessageKey key, string? provider = null) =>
        Strings.TryGetValue(key, out var text) ? text : key.ToString();
}

builder.Services.AddAuthKitSupabase(supabase => { /* … */ },
    ui => ui.Messages = new PtBrMessages());
```

```ts
provideAuthKitSupabase(config, {
  messages: {
    get: (key) => PT_BR[key] ?? key,
  },
});
```

The full key list is `AuthMessageKey` in the contract file.

## Component reference

### Blazor

| Component              | Parameters                                                                                                         | Callbacks                                                                |
| ---------------------- | ------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------ |
| `AuthKitEmail`         | `ShowConfirmPasswordField`, `MetadataFields`, `PrefilledEmail`, `PrefilledPassword`, `RedirectTo`, `IncludeStyles` | `OnSignInComplete`, `OnSignUpComplete`, `OnPasswordResetSent`, `OnError` |
| `AuthKitMagicLink`     | `PrefilledEmail`, `RedirectTo`, `IncludeStyles`                                                                    | `OnSent`, `OnError`                                                      |
| `AuthKitResetPassword` | `PrefilledEmail`, `UpdateMode`, `RedirectTo`, `IncludeStyles`                                                      | `OnResetRequested`, `OnPasswordUpdated`, `OnError`                       |
| `AuthKitSocials`       | `Providers`, `Colored`, `Variant`, `RedirectTo`, `IncludeStyles`                                                   | `OnOAuthRequested`, `OnError`                                            |
| `AuthKitCallback`      | `RedirectTo`, `IncludeStyles`                                                                                      | `OnCompleted`, `OnError`                                                 |

### Angular

| Component           | Inputs                                                                                            | Outputs                                                           |
| ------------------- | ------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------- |
| `ak-email`          | `metadataFields`, `showConfirmPasswordField`, `prefilledEmail`, `prefilledPassword`, `redirectTo` | `signInComplete`, `signUpComplete`, `passwordResetSent`, `failed` |
| `ak-magic-link`     | `prefilledEmail`, `redirectTo`                                                                    | `sentLink`, `failed`                                              |
| `ak-reset-password` | `updateMode`, `prefilledEmail`, `redirectTo`                                                      | `resetRequested`, `passwordUpdated`, `failed`                     |
| `ak-socials`        | `providers`, `colored`, `variant`, `redirectTo`                                                   | `oauthRequested`, `failed`                                        |
| `ak-callback`       | `redirectTo`                                                                                      | `completedSession`, `failed`                                      |

## Rules that keep the security guarantees

1. **Never store tokens yourself.** Use the provider; it owns storage and refresh.
2. **Never display `error.Message` from the provider exception.** `AuthResult.Error.Message` is
   already mapped; raw provider text can leak internals.
3. **Do not navigate from inside a component.** Emit the callback and let the host decide.
4. **Keep the redirect URL in configuration**, never in a query parameter.
5. **Server-side Blazor**: never move the session to `sessionStorage`; the refresh token must stay
   on the server.

## See it running

```bash
bash scripts/poc-up.sh   # http://127.0.0.1:8088
```

The demo in `projects/demo` uses the library with a custom theme and is a working reference for
every level above.
