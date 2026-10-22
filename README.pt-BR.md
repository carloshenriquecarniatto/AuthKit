# AuthKit

[English](README.md) · **Português** · [Español](README.es.md)

Telas de autenticação prontas em uma linha, nativas do seu stack.

O AuthKit é um kit de UI de autenticação independente de provider, com **Supabase como provider
padrão**. Ele entrega o mesmo contrato para .NET e TypeScript, o mesmo modelo de componentes do
[`supabase_auth_ui`](https://pub.dev/packages/supabase_auth_ui) e nenhum interop de React.

```text
AuthKit.Core        contratos, modelos, máquina de estados, validação, tema   (sem dependências de terceiros)
AuthKit.Supabase    provider Supabase Auth
AuthKit.Blazor      componentes Blazor nativos (WebAssembly, Server, Auto)
@authkit/angular    componentes Angular nativos (standalone, signals, v22)
```

## Personalizando a interface

São três níveis, e o [site de documentação](https://carloshenriquecarniatto.github.io/AuthKit/)
explica cada um com código para Blazor e Angular:

- **English** —
  [Customizing the UI](https://carloshenriquecarniatto.github.io/AuthKit/customization/)
- **Português** —
  [Personalizando a interface](https://carloshenriquecarniatto.github.io/AuthKit/personalizacao/)
- **Español** —
  [Personalizar la interfaz](https://carloshenriquecarniatto.github.io/AuthKit/personalizacion/)

Em resumo: tokens de tema para cores e espaçamento, classes CSS `ak-*` para layout e o provider
headless (`IAuthProvider` / `AuthProvider`) quando você quer desenhar o formulário.

## Por quê

- **Um contrato, dois SDKs.** `contracts/authkit.contract.json` é normativo. Um teste de paridade em
  cada linguagem quebra o build se algum lado divergir.
- **Renderização nativa.** Componentes Razor para Blazor, componentes Angular para Angular. Sem
  React dentro de um webview, sem interop de JS além do `sessionStorage`.
- **Independente de provider.** `IAuthProvider` / `AuthProvider` é a única dependência da UI. O
  Supabase vem primeiro; Auth0, Firebase e outros entram sem tocar em componente.
- **Segurança faz parte do contrato.** Token de servidor nunca chega ao navegador, a sessão vive em
  `sessionStorage` (nunca `localStorage`), chave service-role é recusada no startup e texto de erro
  do provider nunca chega à UI. Veja [docs/threat-model.md](docs/threat-model.md).

## Quick start — Blazor

```bash
dotnet add package AuthKit.Blazor
dotnet add package AuthKit.Supabase
```

```csharp
builder.Services.AddAuthKitSupabase(
    supabase =>
    {
        supabase.Url = "https://seu-projeto.supabase.co";
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

Blazor Server precisa usar `AuthSessionStorageMode.Memory` e manter o refresh token no servidor.
Veja [docs/architecture.md](docs/architecture.md).

## Quick start — Angular

```bash
npm install @authkit/angular @supabase/supabase-js
```

```ts
bootstrapApplication(AppComponent, {
  providers: [
    provideAuthKitSupabase(
      {
        url: 'https://seu-projeto.supabase.co',
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

O repositório traz uma suíte adversarial que ataca a implementação, não o protocolo: vazamento de
token, isolamento entre circuits, adulteração de storage, injeção de CSS/markup, ReDoS, amplificação
de requisições, replay de OAuth e configuração insegura.

```bash
dotnet test security/AuthKit.Pentest/AuthKit.Pentest.csproj
bash scripts/pentest-live.sh    # Docker: stack isolado, credenciais por execução, RLS incluso
```

66 ataques offline rodam em todo pull request e bloqueiam release. O runner live sobe um Supabase
isolado com **credenciais por execução** — JWT secret, senha do banco e anon key são gerados, nunca
commitados e apagados no teardown — e ataca autenticação, ciclo de vida do token e row level
security via PostgREST com duas identidades reais. Resultados e achados estão em
[docs/pentest.md](docs/pentest.md).

## Teste local

```bash
bash scripts/poc-up.sh
```

Sobe o stack Supabase isolado (credenciais por execução), builda a biblioteca Angular e o app demo,
e serve **http://127.0.0.1:8088**. Cadastre-se com qualquer e-mail — a confirmação está desligada
nesse stack — e crie notas. Cada conta só vê as próprias linhas, garantido por RLS no Postgres. A
sessão fica em `sessionStorage`; faça sign out e confira a aba de storage.

```bash
bash scripts/poc-down.sh   # para a demo e destrói o stack
```

## Status

| Área               | Estado                                                                        |
| ------------------ | ----------------------------------------------------------------------------- |
| Contrato (`1.0.0`) | Estável                                                                       |
| `AuthKit.Core`     | E-mail, magic link, OTP (e-mail + telefone), recuperação, OAuth, capabilities |
| `AuthKit.Supabase` | Provider completo, mapeamento de erros, restauração de sessão                 |
| `AuthKit.Blazor`   | E-mail, magic link, reset, socials, callback                                  |
| `@authkit/angular` | E-mail, magic link, reset, socials, callback                                  |
| MAUI               | Planejado (v0.2): mesmo core, shell XAML, `WebAuthenticator` + targets        |

## Estrutura do repositório

```
contracts/            contrato normativo compartilhado pelos dois SDKs
src/dotnet/           AuthKit.Core, AuthKit.Supabase, AuthKit.Blazor
src/angular/          workspace Angular e @authkit/angular
tests/dotnet/         xUnit, bUnit e integração com transporte stub
docs/                 spec, threat model, arquitetura, paridade de UI
```

## Contribuindo

Leia [CONTRIBUTING.md](CONTRIBUTING.md). Commits seguem Conventional Commits, releases são
automáticos e todo pull request precisa passar testes, Semgrep, Trivy, npm audit e NuGet audit.

## Segurança

Reporte vulnerabilidades em privado, como descrito em [SECURITY.md](SECURITY.md). Não abra issue
pública para suspeita de exposição de token ou sessão.
