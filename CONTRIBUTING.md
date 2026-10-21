# Contributing to AuthKit

## Ground rules

1. **Everything is in English** — code, comments, documentation, commit messages, issues and PRs.
2. **The contract is normative.** Any change to `contracts/authkit.contract.json` must be applied to
   both SDKs in the same pull request, and both parity tests must pass.
3. **No business logic in a renderer.** Validation, state transitions and error mapping belong to
   the core.
4. **Security issues are never discussed in a public issue.** See [SECURITY.md](SECURITY.md).
5. **One logical change per pull request.** Small PRs get reviewed; big PRs rot.

## Setup

```bash
# .NET 10 SDK is required (see global.json)
dotnet workload restore

# Angular workspace
cd src/angular/authkit-workspace && npm install

# Git hooks (required)
./scripts/setup-hooks.sh
```

The hooks are also installed automatically by `npm install` through the `prepare` script.

## Build and test

```bash
dotnet build
dotnet test

cd src/angular/authkit-workspace
npm run build
npm test
```

### Known local limitation

Vite and the Angular unit-test builder cannot run from a path containing `#`. The repository root
here is `Dev/C#`, so run Angular commands from a copy or a worktree without that character. CI is
unaffected. This is an upstream limitation, not an AuthKit bug.

## Commit messages

Conventional Commits, enforced by `commitlint` in the `commit-msg` hook:

```
feat(blazor): add phone OTP component
fix(supabase): map otp_expired to the correct error code
docs(security): document the XSS residual risk
chore(ci): cache NuGet packages
```

Allowed types: `feat`, `fix`, `perf`, `refactor`, `docs`, `test`, `build`, `ci`, `chore`, `revert`.

The version is derived from these messages. `feat` bumps the minor, `fix`/`perf` bump the patch, and
`BREAKING CHANGE:` bumps the major.

## Pull request checklist

- [ ] Tests cover the new behaviour (unit, and integration when the provider is involved).
- [ ] `dotnet test` passes locally.
- [ ] `npm test` and `npm run build` pass in the Angular workspace.
- [ ] No new token, URL or header is written to a log.
- [ ] If the contract changed, both SDKs and both parity tests changed.
- [ ] Documentation updated (`docs/`, README or XML/TSDoc comments).
- [ ] No secrets, `.env` files or private keys in the diff.

## Hooks

| Hook         | What it blocks                                                                                 |
| ------------ | ---------------------------------------------------------------------------------------------- |
| `commit-msg` | Non-conventional commit messages                                                               |
| `pre-commit` | Secrets, private keys, `.env` files, files over 2 MB, trailing whitespace, generated artifacts |
| `pre-push`   | Failing .NET tests; failing Angular tests when `node_modules` is present                       |

Escape hatch: `AUTHKIT_SKIP_HOOKS=1 git commit ...`. CI runs the same checks, so skipping only moves
the failure.

## Changing the contract

1. Edit `contracts/authkit.contract.json`.
2. Update `AuthKit.Core` models, `AuthContractValues` and the C# parity test fixtures.
3. Update `auth.models.ts`, `AUTHKIT_CONTRACT` and the TS parity test.
4. Update `docs/spec.md` and `docs/ui-parity.md` when behaviour changes.
5. Bump `contractVersion` for breaking changes.

## Releasing

Releases are automatic on `main`. The workflow runs tests and every security scanner first; if any
of them fails, no tag, no package, no changelog entry is produced. Never bump a version by hand.
