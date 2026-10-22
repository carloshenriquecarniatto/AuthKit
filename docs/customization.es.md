---
description:
  'Cómo personalizar AuthKit: tokens de tema, CSS y provider headless, para Blazor y Angular.'
lang: es
alt_lang:
  en: /customization/
  pt: /personalizacao/
  es: /personalizacion/
layout: page
title: Personalizar la interfaz
permalink: /personalizacion/
---

[English]({{ site.baseurl }}/customization/) · [Português]({{ site.baseurl }}/personalizacao/) ·
**Español**

AuthKit te da una pantalla de login funcionando en una línea, pero nada en ella está cerrado. Hay
tres niveles de personalización y puedes detenerte en el que resuelva tu problema.

| Nivel             | Qué cambias                             | Qué mantienes                                  | Esfuerzo |
| ----------------- | --------------------------------------- | ---------------------------------------------- | -------- |
| 1. Tokens de tema | Colores, radio, espaciado, fuente, logo | Todos los componentes y comportamientos        | Minutos  |
| 2. CSS            | Layout, densidad, sombras, animaciones  | Todos los componentes y comportamientos        | Horas    |
| 3. Headless       | La interfaz completa                    | Provider, storage, mapeo de errores, seguridad | Días     |

![Formulario de acceso]({{ site.baseurl }}/assets/login.png)

## Nivel 1 — tokens de tema

`AuthTheme` es un record simple con diez tokens. Ambos SDK usan los mismos nombres y, en la web, se
convierten en custom properties de CSS en el elemento raíz del componente.

| Token        | Variable CSS       | Valor por defecto |
| ------------ | ------------------ | ----------------- |
| `Primary`    | `--ak-primary`     | `#4f46e5`         |
| `Background` | `--ak-background`  | `#ffffff`         |
| `Surface`    | `--ak-surface`     | `#ffffff`         |
| `Text`       | `--ak-text`        | `#111827`         |
| `TextMuted`  | `--ak-text-muted`  | `#6b7280`         |
| `Border`     | `--ak-border`      | `#d1d5db`         |
| `Danger`     | `--ak-danger`      | `#dc2626`         |
| `Radius`     | `--ak-radius`      | `8px`             |
| `Spacing`    | `--ak-spacing`     | `16px`            |
| `FontFamily` | `--ak-font-family` | stack del sistema |

### Blazor

```csharp
builder.Services.AddAuthKitSupabase(
    supabase =>
    {
        supabase.Url = "https://tu-proyecto.supabase.co";
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

Los valores del tema se sanean: se eliminan `; { } < > " ' \` y se rechazan `url(`/`expression(`,
así que un tema que venga de entrada del usuario no inyecta CSS. `LogoUrl` solo acepta https o una
ruta relativa.

## Nivel 2 — tu propio CSS

Todas las clases llevan el prefijo `ak-`. Sobrescríbelas globalmente o desactiva el CSS incluido y
escribe el tuyo.

```css
/* Cargado después del CSS de AuthKit, o con IncludeStyles="false" en Blazor. */
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

Puntos útiles: `.ak-form`, `.ak-field`, `.ak-label`, `.ak-input`, `.ak-hint`, `.ak-button`,
`.ak-link`, `.ak-error`, `.ak-notice`, `.ak-socials`, `.ak-social`, `.ak-spinner`.

### Blazor

`<AuthKitEmail IncludeStyles="false" />` deja de emitir la hoja base; incluye tu CSS en la página
anfitriona. Todos los componentes aceptan `IncludeStyles`.

### Angular

Los componentes usan `ViewEncapsulation.None`, así que un stylesheet global de tu aplicación
sobrescribe las clases `ak-*` directamente.

## Nivel 3 — headless

Cuando el diseño no se parece a una tarjeta de login, deja de usar los componentes y usa el
provider. Conservas lo difícil de acertar: mapeo de errores, storage de la sesión, refresh y
validación.

### Blazor

```razor
@inject IAuthProvider Auth
@inject NavigationManager Nav

<form @onsubmit="Submit" @onsubmit:preventDefault class="my-login">
    <input @bind="_email" placeholder="tu@empresa.com" />
    <input type="password" @bind="_password" />
    <button disabled="@_busy">@(_busy ? "…" : "Entrar")</button>
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
            // Ya es seguro mostrarlo: el SDK mapea el texto del provider.
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
      <button [disabled]="busy">Entrar</button>
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
    // result.error?.message es seguro para mostrar.
  }
}
```

El provider también expone `capabilities` (lo que soporta el proyecto),
`onStateChange`/`StateChanged` y toda la superficie de operaciones listada en
`contracts/authkit.contract.json`.

## Mensajes e idioma

Inglés viene incluido. Reemplaza la tabla para traducir o reescribir cualquier texto por defecto.

```csharp
public sealed class EsMessages : IAuthMessages
{
    private static readonly Dictionary<AuthMessageKey, string> Strings = new()
    {
        [AuthMessageKey.SignIn] = "Entrar",
        [AuthMessageKey.Email] = "Tu correo",
        [AuthMessageKey.Password] = "Tu contraseña",
        [AuthMessageKey.InvalidEmail] = "Introduce un correo válido",
        [AuthMessageKey.PasswordTooShort] =
            "La contraseña debe tener al menos {min} caracteres",
    };

    public string Resolve(AuthMessageKey key, string? provider = null) =>
        Strings.TryGetValue(key, out var text) ? text : key.ToString();
}

builder.Services.AddAuthKitSupabase(supabase => { /* … */ },
    ui => ui.Messages = new EsMessages());
```

```ts
provideAuthKitSupabase(config, {
  messages: {
    get: (key) => ES[key] ?? key,
  },
});
```

La lista completa de claves es `AuthMessageKey`, en el archivo de contrato.

## Referencia de componentes

### Blazor

| Componente             | Parámetros                                                                                                         | Callbacks                                                                |
| ---------------------- | ------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------ |
| `AuthKitEmail`         | `ShowConfirmPasswordField`, `MetadataFields`, `PrefilledEmail`, `PrefilledPassword`, `RedirectTo`, `IncludeStyles` | `OnSignInComplete`, `OnSignUpComplete`, `OnPasswordResetSent`, `OnError` |
| `AuthKitMagicLink`     | `PrefilledEmail`, `RedirectTo`, `IncludeStyles`                                                                    | `OnSent`, `OnError`                                                      |
| `AuthKitResetPassword` | `PrefilledEmail`, `UpdateMode`, `RedirectTo`, `IncludeStyles`                                                      | `OnResetRequested`, `OnPasswordUpdated`, `OnError`                       |
| `AuthKitSocials`       | `Providers`, `Colored`, `Variant`, `RedirectTo`, `IncludeStyles`                                                   | `OnOAuthRequested`, `OnError`                                            |
| `AuthKitCallback`      | `RedirectTo`, `IncludeStyles`                                                                                      | `OnCompleted`, `OnError`                                                 |

### Angular

| Componente          | Inputs                                                                                            | Outputs                                                           |
| ------------------- | ------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------- |
| `ak-email`          | `metadataFields`, `showConfirmPasswordField`, `prefilledEmail`, `prefilledPassword`, `redirectTo` | `signInComplete`, `signUpComplete`, `passwordResetSent`, `failed` |
| `ak-magic-link`     | `prefilledEmail`, `redirectTo`                                                                    | `sentLink`, `failed`                                              |
| `ak-reset-password` | `updateMode`, `prefilledEmail`, `redirectTo`                                                      | `resetRequested`, `passwordUpdated`, `failed`                     |
| `ak-socials`        | `providers`, `colored`, `variant`, `redirectTo`                                                   | `oauthRequested`, `failed`                                        |
| `ak-callback`       | `redirectTo`                                                                                      | `completedSession`, `failed`                                      |

## Reglas que preservan las garantías de seguridad

1. **Nunca guardes tokens por tu cuenta.** Usa el provider; él gestiona storage y refresh.
2. **Nunca muestres el mensaje de la excepción del provider.** `AuthResult.Error.Message` ya viene
   mapeado; el texto crudo filtra detalles internos.
3. **No navegues desde dentro del componente.** Emite el callback y deja que el host decida.
4. **Mantén la URL de redirect en la configuración**, nunca en un parámetro de query.
5. **Blazor Server**: nunca muevas la sesión a `sessionStorage`; el refresh token se queda en el
   servidor.

## Verlo funcionando

```bash
bash scripts/poc-up.sh   # http://127.0.0.1:8088
```

La demo en `projects/demo` usa la librería con tema personalizado y sirve de referencia para todos
los niveles anteriores.

---

[Markdown source](https://github.com/carloshenriquecarniatto/AuthKit/blob/main/docs/customization.es.md)
· [llms.txt]({{ site.baseurl }}/llms.txt)
