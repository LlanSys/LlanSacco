# Phase 0 - Git Bootstrap And Baseline

Date: 2026-09-10
Status: Git/CI foundation implemented; import remains draft with a failing application baseline.

## Repository

The remote already contained main commit 4b5a438 (Initial commit, .gitignore only). Local Git was initialized against that history without replacing working files. Work uses import/existing-application; main was not force-pushed. The remote was observed public at phase start; this work did not change visibility. BaseTemplate reference: c17631e223bc2bd1d86c0d052668a0fee8ebbd31.

Guardrails ruleset 22747730 is active on the default branch. It requires pull requests, resolved conversations, and the GitHub Actions check Required / Remediation against an up-to-date branch; prevents deletion and force pushes; and has no bypass actors. Existing Copilot review configuration was preserved. Squash merging and deletion of merged branches are enabled; merge/rebase methods disabled. The existing zero-human-approval requirement was preserved; reviewer ownership is a Phase 1 decision, not a claim of independent human approval.

## Import Safety

- Deployment/provisioning push triggers removed. Manual execution requires LLANSACCO_DEPLOYMENT_ENABLED=true.
- Image pushing requires manual dispatch, publish_images, and LLANSACCO_PUBLISH_ENABLED=true.
- No deployment, migration, provisioning, or publishing was performed.
- Nine non-empty development credential/connection/license settings were moved to the API user-secrets store, preserving existing flat-key overrides. Committed Development fields are empty. New developers must supply their settings locally.
- Local scratch scripts, task.md, TestSerialize.cs, build outputs, logs, environment files, and private-key artifacts are excluded. Files were preserved on disk.
- Gitleaks 8.30.1 staged scan passed over approximately 18.36 MB. Exact exceptions cover a documentation placeholder, the public Azure Key Vault role identifier, and a public Azurite emulator key. This is a scanner result, not a guarantee that every possible secret format is detectable.

## Local Verification

Executed scripts/verify-baseline.ps1 -Check All -NoRestore (Release), and a separate API build with --no-restore -p:UseSharedCompilation=false. The script attempts all checks and fails if any fail; it does not hide failures. PowerShell syntax parsing passed.

| Check | Result |
| --- | --- |
| API build | Failed: three existing CS1061 errors in Dividends queries |
| Blazor build | Failed: CS1513, missing closing brace in IFinanceService.cs line 17 |
| Unit tests | Blocked by backend compilation; tests did not run |
| Architecture tests | Blocked by backend compilation; tests did not run |
| Integration tests | Blocked by backend compilation; tests did not run |

Dividends errors: GetDividendDeclarationsQuery.cs and GetDividendPreferencesQuery.cs call GetAllAsync; GetDividendDeclarationByIdQuery.cs calls GetByIdAsync. These are absent from IRepository. No replacement methods were added to dilute the established contract. Resolve through existing generic methods during remediation.

Also observed: NU1608 OpenAPI dependency mismatch and extensive analyzer warnings. The complete import has 1,507 git diff --check diagnostics (trailing whitespace/EOF), retained as legacy baseline rather than performing an unrelated whole-tree rewrite. Local output is in ignored artifacts-phase0.log. Tests cannot establish provider correctness until compilation succeeds.

## CI And Closure

.github/workflows/remediation-ci.yml runs independent API, Blazor, Unit, Architecture, Integration, and Secrets checks on every PR to main; it has no path filter. Required / Remediation succeeds only if all prerequisites succeed. The scanner download verifies the pinned release checksum. scripts/verify-baseline.ps1 is the shared local/CI entry point. Existing inherited checks remain enabled.

The import must remain draft/unmerged while its checks fail. Application correctness, architecture, cache, and naming remediation remain pending; a failing baseline is evidence, not permission to bypass the ruleset. Remote run/PR evidence will be recorded after the first push.
