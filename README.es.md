# AuthKit

[English](README.md) · [Português](README.pt-BR.md) · **Español**

Pantallas de autenticación listas en una línea, nativas de tu stack.

AuthKit es un kit de UI de autenticación independiente del provider, con **Supabase como provider
predeterminado**. Entrega el mismo contrato para .NET y TypeScript, el mismo modelo de componentes
de [`supabase_auth_ui`](https://pub.dev/packages/supabase_auth_ui) y ningún interop de React.

```text
AuthKit.Core        contratos, modelos, máquina de estados, validación, tema   (sin dependencias de terceros)
AuthKit.Supabase    provider Supabase Auth
AuthKit.Blazor      componentes Blazor nativos (WebAssembly, Server, Auto)
@authkit/angular    componentes Angular nativos (standalone, signals, v22)
```

## Personalizar la interfaz

Hay tres niveles, y el [sitio de documentación](https://carloshenriquecarniatto.github.io/AuthKit/)
explica cada uno con código para Blazor y Angular:

- **English** —
  [Customizing the UI](https://carloshenriquecarniatto.github.io/AuthKit/customization/)
- **Português** —
  [Personalizando a interface](https://carloshenriquecarniatto.github.io/AuthKit/personalizacao/)
- **Español** —
  [Personalizar la interfaz](https://carloshenriquecarniatto.github.io/AuthKit/personalizacion/)

En resumen: tokens de tema para colores y espaciado, clases CSS `ak-*` para el layout y el provider
headless (`IAuthProvider` / `AuthProvider`) cuando quieres dibujar el formulario.

## Por qué

- **Un contrato, dos SDK.** `contracts/authkit.contract.json` es normativo. Un test de paridad en
  cada lenguaje rompe el build si algún lado se desvía.
- **Renderizado nativo.** Componentes Razor para Blazor, componentes Angular para Angular. Sin React
  dentro de un webview, sin interop de JS más allá de `sessionStorage`.
- **Independiente del provider.** `IAuthProvider` / `AuthProvider` es la única dependencia de la UI.
  Supabase viene primero; Auth0, Firebase y otros entran sin tocar componentes.
- **La seguridad es parte del contrato.** El token de servidor nunca llega al navegador, la sesión
  vive en `sessionStorage` (nunca `localStorage`), la clave service-role se rechaza al arrancar y el
  texto de error del provider nunca llega a la UI. Ver [docs/threat-model.md](docs/threat-model.md).

## Quick start — Blazor

```bash
dotnet add package AuthKit.Blazor
dotnet add package AuthKit.Supabase
```

```csharp
builder.Services.AddAuthKitSupabase(
    supabase =>
    {
        supabase.Url = "https://tu-proyecto.supabase.co";
        supabase.AnonKey = "<anon key>";
        supabase.RedirectTo = "https://app.example.com/auth/callback";
        supabase.SocialProviders = ["google", "github"];
    },
    ui => ui.Theme = AuthTheme.Default with { Primary = "#0f766e" });

builder.Services.AddAuthKitBlazor(AuthSessionStorageMode.BrowserSession);
```

```razor
@page "/login"

<AuthKitEmail OnSignInComplete="GoHome" />
<AuthKitSocials />

@code {
    void GoHome(AuthSession session) => Navigation.NavigateTo("/");
}
```

Blazor Server debe usar `AuthSessionStorageMode.Memory` y mantener el refresh token en el servidor.
Ver [docs/architecture.md](docs/architecture.md).

## Quick start — Angular

```bash
npm install @authkit/angular @supabase/supabase-js
```

```ts
bootstrapApplication(AppComponent, {
  providers: [
    provideAuthKitSupabase(
      {
        url: 'https://tu-proyecto.supabase.co',
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

## Pentest

El repositorio incluye una suite adversarial que ataca la implementación, no el protocolo: fuga de
tokens, aislamiento entre circuits, manipulación del storage, inyección de CSS/markup, ReDoS,
amplificación de peticiones, replay de OAuth y configuración insegura.

```bash
dotnet test security/AuthKit.Pentest/AuthKit.Pentest.csproj
bash scripts/pentest-live.sh    # Docker: stack aislado, credenciales por ejecución, RLS incluido
```

66 ataques offline corren en cada pull request y bloquean el release. El runner live levanta un
Supabase aislado con **credenciales por ejecución** — JWT secret, contraseña de la base y anon key
se generan, nunca se commitean y se borran en el teardown — y ataca autenticación, ciclo de vida del
token y row level security vía PostgREST con dos identidades reales. Resultados y hallazgos en
[docs/pentest.md](docs/pentest.md).

## Pruébalo localmente

```bash
bash scripts/poc-up.sh
```

Levanta el stack Supabase aislado (credenciales por ejecución), compila la librería Angular y la app
demo, y sirve **http://127.0.0.1:8088**. Regístrate con cualquier correo — la confirmación está
desactivada en este stack — y crea notas. Cada cuenta solo ve sus propias filas, garantizado por RLS
en Postgres. La sesión vive en `sessionStorage`; cierra sesión y revisa la pestaña de storage.

```bash
bash scripts/poc-down.sh   # detiene la demo y destruye el stack
```

## Estado

| Área               | Estado                                                                       |
| ------------------ | ---------------------------------------------------------------------------- |
| Contrato (`1.0.0`) | Estable                                                                      |
| `AuthKit.Core`     | Email, magic link, OTP (email + teléfono), recuperación, OAuth, capabilities |
| `AuthKit.Supabase` | Provider completo, mapeo de errores, restauración de sesión                  |
| `AuthKit.Blazor`   | Email, magic link, reset, socials, callback                                  |
| `@authkit/angular` | Email, magic link, reset, socials, callback                                  |
| MAUI               | Planeado (v0.2): mismo core, shell XAML, `WebAuthenticator` + targets        |

## Estructura del repositorio

```
contracts/            contrato normativo compartido por ambos SDK
src/dotnet/           AuthKit.Core, AuthKit.Supabase, AuthKit.Blazor
src/angular/          workspace Angular y @authkit/angular
tests/dotnet/         xUnit, bUnit e integración con transporte stub
docs/                 spec, threat model, arquitectura, paridad de UI
```

## Contribuir

Lee [CONTRIBUTING.md](CONTRIBUTING.md). Los commits siguen Conventional Commits, los releases son
automáticos y todo pull request debe pasar tests, Semgrep, Trivy, npm audit y NuGet audit.

## Seguridad

Reporta vulnerabilidades en privado, como se describe en [SECURITY.md](SECURITY.md). No abras un
issue público por una sospecha de exposición de token o sesión.
