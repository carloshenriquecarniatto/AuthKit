# Threat model

Scope: the AuthKit SDKs for Blazor, Angular and (from v0.2) MAUI, plus the Supabase provider.
Assets: access tokens, refresh tokens, user identity, and the trust relationship between the UI and
the Supabase project.

Method: STRIDE applied per trust boundary, with the mitigation that actually exists in this
repository. "Tested" means a test in `tests/dotnet` or `projects/authkit/src/**/*.spec.ts` fails if
the mitigation is removed.

| ID   | Threat                                                                                                              | Mitigation                                                                                                                                     | Tested                                                                              |
| ---- | ------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------- |
| T-01 | **Cross-user session leakage in Blazor Server** — a singleton client shares one user's session with another circuit | `SupabaseClient`, provider and session store are registered **scoped**; a scope maps to a circuit                                              | DI test asserts scoped registration                                                 |
| T-02 | **Refresh token reaches the browser on Server hosting**                                                             | Server hosting uses `InMemoryAuthSessionStore`; the browser store is opt-in and documented as WebAssembly-only                                 | Documentation + store selection                                                     |
| T-03 | **Token theft through XSS**                                                                                         | `sessionStorage` (not `localStorage`), short access-token TTL, refresh rotation, no tokens in URLs after callback; CSP guidance in SECURITY.md | Storage unit test, callback test                                                    |
| T-04 | **Service-role key shipped to a client**                                                                            | Startup validation decodes the JWT and refuses `service_role`                                                                                  | `Service_role_keys_are_rejected`, `refuses a service role key`                      |
| T-05 | **Session survives longer than intended**                                                                           | Expiry checked with a 30 second skew; expired payloads are deleted on load                                                                     | `Expiry_uses_a_thirty_second_skew`                                                  |
| T-06 | **Credential leakage in logs**                                                                                      | `AuthSession` overrides `ToString`/`PrintMembers` to redact both tokens; no provider logs token material                                       | `To_string_redacts_tokens`                                                          |
| T-07 | **Provider error text leaks internals**                                                                             | Errors are mapped by machine code/status to AuthKit-owned strings; raw `error_description` is dropped                                          | `Invalid_credentials_..._server_text_is_not_forwarded`                              |
| T-08 | **Style injection via theme values**                                                                                | Theme tokens strip `; { } < > " ' \` and reject `url(`/`expression(`; logo URLs must be https or relative                                      | `Style_injection_attempts_are_neutralised`, `Remote_fetches_are_rejected_in_tokens` |
| T-09 | **Open redirect through OAuth**                                                                                     | The redirect URL is configuration, never user input; the provider allowlist lives in Supabase                                                  | Design                                                                              |
| T-10 | **ReDoS through validation**                                                                                        | Bounded, non-backtracking email pattern; length capped at 254                                                                                  | `Email_validation_rejects_hostile_input_quickly`                                    |
| T-11 | **Request amplification / brute force**                                                                             | `AuthFlow` rejects concurrent submissions; provider rate-limit errors are surfaced, not retried automatically                                  | `Concurrent_submissions_are_rejected...`                                            |
| T-12 | **OAuth code replay**                                                                                               | PKCE with a verifier stored per attempt; the verifier is consumed on callback; codes are single-use                                            | `completeOAuth` implementation                                                      |
| T-13 | **Tampered session payload**                                                                                        | Corrupt or unparsable stored sessions are treated as signed out, never as an error                                                             | `BrowserSessionStore.LoadAsync`                                                     |
| T-14 | **Supply chain**                                                                                                    | Semgrep, Trivy, npm audit, NuGet audit block the release workflow                                                                              | `.github/workflows/security.yml`                                                    |

## Residual risks (accepted for 0.x)

1. **XSS on the host application defeats `sessionStorage`.** No client-side storage can defend
   against script running on the same origin. Compensating controls: CSP, short tokens, rotation.
2. **A compromised anon key is not a vulnerability.** The anon key is public by design; security
   depends on row level security in the database. AuthKit cannot enforce RLS.
3. **OAuth client secrets** belong to the provider console, never to these packages.

## Reporting

Security issues follow [SECURITY.md](../SECURITY.md). The threat model is reviewed whenever a
provider or hosting model is added.
