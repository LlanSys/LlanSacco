# CI/CD Workflow and Troubleshooting

This repository protects `main`. Treat `main` as the integration/release branch, not the everyday development branch.

## Standard Team Cycle

1. Start from an updated `main`.

   ```powershell
   git switch main
   git pull --ff-only origin main
   ```

2. Create a feature branch.

   ```powershell
   git switch -c feat/short-description
   ```

3. Make changes locally.

4. Run local guardrails before pushing.

   ```powershell
   ./scripts/verify-baseline.ps1 -BaseRef origin/main
   ```

5. Push the feature branch.

   ```powershell
   git push -u origin feat/short-description
   ```

6. Open a pull request into `main`.

7. Wait for **Required / Remediation** and review the evidence attached to the run.

8. Resolve review comments and conversations.

9. Squash-merge the pull request. GitHub deletes the merged branch.

10. Update local `main`.

    ```powershell
    git switch main
    git pull --ff-only origin main
    ```

## Why Direct Pushes to Main Fail

Direct pushes to `main` are intentionally blocked by branch protection.

Typical error:

```text
GH006: Protected branch update failed for refs/heads/main.
- Changes must be made through a pull request.
- Required status check "Required / Remediation" is expected.
```

This is not a build failure. It means GitHub rejected the push because `main` only accepts changes through a pull request.

Fix:

```powershell
git switch -c feat/short-description
git push -u origin feat/short-description
```

Then open a PR into `main`.

## GitHub Actions Failure Checklist

When a GitHub Actions run fails, check in this order.

1. Confirm which workflow failed.

   Open the failed run in GitHub Actions and note the workflow name, job name, and failing step.

2. Confirm GitHub is running the workflow file you expect.

   Compare the run’s head commit with the commit that contains the fix. Push local workflow changes to the PR branch before expecting GitHub to test them.

3. Check whether the failure is from restore, build, or tests.

   Restore failures usually mean dependency, workload, SDK, or NuGet source issues.

   Build failures usually mean compile errors, missing references, or incompatible target frameworks.

   Test failures usually mean guardrails or behavioral tests intentionally blocked the change.

4. If the error mentions MAUI workloads, check workflow scope first.

   Backend guardrail workflows should not restore the whole solution unless they intentionally validate mobile projects too.

   Full-solution or mobile workflows should install/restore the required MAUI workloads for supported platforms.

5. If the error mentions a required check is expected, check branch protection.

   The required check name must match the GitHub Actions job name exactly.

6. If local checks pass but GitHub fails, compare configuration.

   Look for differences in `Debug` vs `Release`, SDK version, OS runner, environment variables, or target framework.

## Current Guardrails

Run `./scripts/verify-baseline.ps1 -BaseRef origin/main` in PowerShell 7 from the checkout. It runs Release builds, unit, architecture and both-provider integration tests, plus the exact debt comparison, package policy and formatter fixtures. Integration tests use disposable Docker databases. Use `-Check Guardrails -BaseRef origin/main` for only the policy checks, or select `Api`, `Blazor`, `Unit`, `Architecture` or `Integration` for a focused run.

The optional pre-push hook calls the same entry point for Debug API and architecture feedback. The hosted required gate adds the Gitleaks history scan and publishes test diagnostics. Request independent review when an eligible reviewer is available. The active solo-maintainer policy retains the required check and resolved review conversations.

## Workflow Naming Convention

Name workflow files by the boundary they protect.

Recommended examples:

```text
remediation-ci.yml       Required / Remediation and its prerequisite jobs.
backend-architecture.yml  Supplemental backend architecture/build feedback.
backend-ci.yml            Backend Release build plus unit/integration tests.
frontend-ci.yml           Blazor/RCL Release build.
mobile-ci.yml             Supported MAUI target builds; currently manual/reusable.
full-solution-ci.yml      Manual/scheduled orchestration across backend/frontend/mobile.
release.yml               Manual Release artifact packaging.
```

Avoid vague names like `ci.yml` once the repository has multiple platforms or deployment paths.

## Debug vs Release

Use `Debug` for fast local feedback and pre-push checks.

Use `Release` in GitHub checks that protect `main`, because `main` represents merge-ready code and should stay close to deployable build conditions.

## Bypassing Local Hooks

Bypassing local hooks should be rare. GitHub branch protection still applies.

```powershell
$env:SKIP_GUARDRAILS="1"
git push
```

Use this only when the local environment is blocked but the PR will still run GitHub checks.
