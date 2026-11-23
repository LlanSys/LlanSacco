# Phase 6 Validators and Slice Layout Verification

Phase 6 starts from merged main at 619555b. The implementation uses the existing LlanSacco checkout.

## Request validation

Application registration explicitly scans internal validators. The runtime inventory covers all 131 command types. Eight commands use ambient identity, an optional discoverable-passkey username, worker state, or server configuration. Their exact exemptions are asserted in Phase6ValidationTests. Each of the remaining 123 commands has a validator resolved from the same service registration used by the API.

The work adds 53 validators for existing input-bearing requests. It reuses the membership request validators and the HR employee request validator where commands wrap those contracts. Validators reject malformed inputs before the handler, and the domain continues to own business decisions such as membership eligibility and account balances. The audit keeps optional administrative reasons and passkey discovery valid.

## Slice ownership

- Move 87 Application commands and their internal handlers into operation-named files. Keep request names and namespaces so persisted MediatR requests retain their identities.
- Move 52 existing validator declarations to feature-owned Validators folders, including the 19 registered standalone-file violations. Validators now activate in the existing validation behaviour.
- Keep the 44 existing IAM and Control Plane infrastructure handlers behind the Application boundary. The exact exception register identifies both source files, the request and handler types, and the reason for each split. Architecture tests reject unused exceptions and confirm that each referenced handler implements the exact request contract.
- Rename ten shared transport types to Request or Response. Dividend transports now live in their owning feature. The two existing request types keep their namespaces so the controller signatures retain their established identity. JSON property names remain compatible, and the CLR and OpenAPI model names follow the new type names.
- Convert the remaining 16 free-form logging calls in the moved slices to source-generated methods.

## EF and platform layout

Split the eight payroll configurations into type-named files and place the twenty misplaced configurations under EntityConfigurations. Those twenty configurations also become internal, closing their visibility findings. The 28 Configure method bodies match merged main. The SQL Server and PostgreSQL Dividends contexts and their factories now live in DataContext. Their historical designers and model snapshots reference the new namespace, preserving migration IDs and definitions. The two provider snapshot comparisons cover their resulting models.

The WinUI code-behind class now matches App.xaml and App.xaml.cs. The XAML declaration uses the matching class, and the MAUI Windows build exercises generated code and shared transport references.

## Verification

| Check | Result |
| --- | --- |
| API and Blazor Release builds | Passed |
| MAUI Windows build | Passed |
| Architecture and migration guards | 86 passed, including both Dividends provider snapshot checks |
| Unit tests | 47 passed |
| Full Release integration suite | 183 passed across SQL Server, PostgreSQL and the other integration fixtures. The existing RabbitMQ transport test is opt-in. |
| Focused checks after the final compatibility adjustment | Three Phase 6 tests passed |
| Formatter, package policy, exact debt comparison and Git whitespace check | Passed |

The register resolves all 181 Phase 6 entries and 20 related EF configuration visibility entries. The 145 remaining findings belong to Phase 7. Hosted Required / Remediation is recorded on the Phase 6 pull request.
