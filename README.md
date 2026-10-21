# AuthKit

Authentication screens that are ready in one line, native to your stack.

AuthKit is a provider-agnostic authentication UI kit with **Supabase as the default provider**. It
ships the same contract for .NET and TypeScript, the same component model as
[`supabase_auth_ui`](https://pub.dev/packages/supabase_auth_ui), and no React interop anywhere.

```text
AuthKit.Core        contracts, models, state machine, validation, theming   (zero dependencies)
AuthKit.Supabase    Supabase Auth provider
AuthKit.Blazor      native Blazor components (WebAssembly, Server, Auto)
@authkit/angular    native Angular components (standalone, signals, v22)
```

## Why

- **One contract, two SDKs.** `contracts/authkit.contract.json` is normative. A parity test in each
  language fails the build if either side drifts.
- **Native rendering.** Razor components for Blazor, Angular components for Angular. No React inside
  a webview, no JS interop beyond `sessionStorage`.
- **Provider-agnostic.** `IAuthProvider` / `AuthProvider` is the only dependency the UI has.
  Supabase ships first; Auth0, Firebase and others can be added without touching a component.
- **Security is part of the contract.** Server-side tokens never reach the browser, sessions live in
  `sessionStorage` (not `localStorage`), service-role keys are rejected at startup, and provider
  error text never reaches the UI. See [docs/threat-model.md](docs/threat-model.md).

## Quick start — Blazor

```bash
dotnet add package AuthKit.Blazor
dotnet add package AuthKit.Supabase
```

```csharp
builder.Services.AddAuthKitSupabase(
    supabase =>
    {
        supabase.Url = "https://your-project.supabase.co";
        supabase.AnonKey = "<anon key>";
        supabase.RedirectTo = "https://app.example.com/auth/callback";
        supabase.SocialProviders = ["google", "github"];
    },
    ui => ui.Theme = AuthTheme.Default with { Primary = "#0f766e" });

builder.Services.AddAuthKitBlazor(AuthSessionStorageMode.BrowserSession);
```

```razor
@page "/login"
@inject AuthKitState Auth

<AuthKitEmail OnSignInComplete="GoHome" />
<AuthKitSocials />

@code {
    void GoHome(AuthSession session) => Navigation.NavigateTo("/");
}
```

Server-side Blazor must use `AuthSessionStorageMode.Memory` and keep refresh tokens on the server.
See [docs/architecture.md](docs/architecture.md).

## Quick start — Angular

```bash
npm install @authkit/angular @supabase/supabase-js
```

```ts
bootstrapApplication(AppComponent, {
  providers: [
    provideAuthKitSupabase(
      {
        url: 'https://your-project.supabase.co',
        anonKey: '<anon key>',
        redirectTo: window.location.origin + '/auth/callback',
        socialProviders: ['google', 'github'],
      },
      { theme: { ...DEFAULT_AUTH_THEME, primary: '#0f766e' } },
    ),
  ],
});
```

```html
<ak-email (signInComplete)="onSignIn($event)" /> <ak-socials />
```

## Status

| Area               | State                                                                        |
| ------------------ | ---------------------------------------------------------------------------- |
| Contract (`1.0.0`) | Stable                                                                       |
| `AuthKit.Core`     | Email, magic link, OTP (email + phone), recovery, OAuth, capabilities        |
| `AuthKit.Supabase` | Complete provider, error mapping, session restore                            |
| `AuthKit.Blazor`   | Email, magic link, reset, socials, callback                                  |
| `@authkit/angular` | Email, magic link, reset, socials, callback                                  |
| MAUI               | Planned (v0.2): same core, XAML shell, `WebAuthenticator` + manifest targets |

## Repository layout

```
contracts/            normative contract shared by both SDKs
src/dotnet/           AuthKit.Core, AuthKit.Supabase, AuthKit.Blazor
src/angular/          Angular workspace and @authkit/angular
tests/dotnet/         xUnit, bUnit and stub-transport integration tests
docs/                 spec, threat model, architecture, UI parity
```

## Contributing

Read [CONTRIBUTING.md](CONTRIBUTING.md). Commits follow Conventional Commits, releases are
automatic, and every pull request must pass tests plus Semgrep, Trivy, npm audit and NuGet audit.

## Security

Report vulnerabilities privately as described in [SECURITY.md](SECURITY.md). Do not open a public
issue for a suspected token or session exposure.
