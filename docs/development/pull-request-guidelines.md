# Pull Request Guidelines

Start with the [PR template](../../.github/pull_request_template.md). Explain the problem, the outcome and the impact on contracts, data, permissions or configuration. Record the checks you ran and their results. Include provider migrations, deployment steps or release blockers when they apply.

Use a short outcome-focused title, such as `Add dynamic menu authorization`. Keep reviewable changes together. For guardrail changes, link the rule, describe how it is enforced and include positive and negative test evidence where applicable.

Read [AGENTS.md](../../AGENTS.md) for the conventions that apply to the change. Update the relevant documentation when a convention changes. Open a focused PR into `main`. Wait for **Required / Remediation** and resolve review conversations before squash-merging. CODEOWNERS identifies the maintainer for architecture and sensitive paths. Request an independent review when another eligible reviewer is available. The single-maintainer policy relies on the required checks and resolved conversations until independent approval is possible.

See the [CI workflow](ci-cd-workflow.md) for the local command and hosted status checks.
