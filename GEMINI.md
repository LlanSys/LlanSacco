# Gemini Instructions

You must strictly follow the rules in this file before generating any code or suggestions. `AGENTS.md` is the absolute canonical source of truth for this repository.

## Pre-Flight Compliance Rules

Before writing code, debugging, or reviewing changes, you MUST execute these steps in your hidden thought process or output:

1. Locate and read `@AGENTS.md`, `@PLAN.md`, and any relevant files in `@docs/architecture/`, `@docs/development/` and `@docs/reference/`.
2. Do not rely on general coding conventions. You must use the specific domain rules found in the files above.
3. CRITICAL: In your very first response sentence, you must output a "Compliance Anchor" in this exact format:
   "Compliance Check: Verified against AGENTS.md rules for [Insert Sacco Component Name Here]."

## Conflict Resolution

Do not duplicate long-lived rules here. If this file and `AGENTS.md` disagree, follow `AGENTS.md` and immediately flag the conflict to the user.
