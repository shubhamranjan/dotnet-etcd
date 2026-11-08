<!-- Title: use a conventional-commit prefix (feat:, fix:, deps:, docs:, ci:, test:, chore:).
     The labeler turns it into the label that groups this PR in the generated release notes.
     Append "!" to the type (e.g. feat!:) or add a "BREAKING CHANGE:" footer below for breaking changes. -->

## What

<!-- One or two sentences on what changes and why. Link the issue, e.g. "Closes #123". -->

## How was it tested

<!-- Unit tests added/updated? Integration tests (need etcd: `dotnet-etcd.Tests/start-etcd.sh`)?
     Paste the relevant `dotnet test` summary line. -->

## Checklist

- [ ] Unit tests cover the change (`--filter "Category=Unit"`)
- [ ] Integration tests pass locally (`--filter "Category=Integration"`), or the change needs none
- [ ] Public API has XML doc comments
- [ ] `docs/` and README updated for user-facing changes
- [ ] No breaking change, or it is called out above and in the title
