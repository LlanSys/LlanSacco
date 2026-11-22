# Phase 4 Application and HTTP Response Verification

The Phase 4 change starts from main at 70111bc, including the merged repository alignment and StarterTemplate package and formatter setup.

## Contract decisions

- Keep the AppResponse<T> envelope aligned with StarterTemplate. Add named factories for specific failures and route feature controllers through the shared Problem Details mapper. Error responses carry their HTTP status, stable code and trace identifier. Payment provider callbacks keep their own acknowledgement format.
- Use typed validation, missing-account, reference-conflict and business-rule outcomes for journal posting. An exact replay returns the existing journal ID. Rejected postings stage no writes. Accounting event consumers retain their support records for rejected events. Payroll closure explicitly stages the mutation before its Unit of Work commits.
- CheckOff validation returns HTTP 200 and an IsSuccess value of true when the operation produced a report. A Data value of false means the report includes rejected rows. Blazor displays a warning and refreshes the batch details. Failed validation operations carry a typed error.
- Blazor retains safe Problem Details messages, error codes and field errors. Provider and exception diagnostics are logged on the server. Unexpected API failures and unreadable downstream responses use safe client messages.

## Verification

The exact debt register removes four Phase 4 entries for untyped failure initializers. The HTTP tests exercise the shared API mapping and Blazor client for 400, 403, 404, 409, 422 and 500 responses, and the CheckOff 200 response with false data. The provider suite covers journal replay and the persisted payroll close, repeat-close conflict and missing-period responses.

| Check | Local result |
| --- | --- |
| API and Blazor builds | Passed |
| Architecture and unit suites | 84 and 45 passed, respectively |
| Architecture debt | Four resolved entries removed, 472 entries remaining in later phases |
| Provider regression run | 172 passed across the integration assembly, including payroll closure on PostgreSQL and SQL Server |
| Opt-in RabbitMQ transport | Passed against the existing LlanSacco Compose broker |
| Package policy and scoped formatter | Passed |
| Required / Remediation | See the pull request for the result on its current commit |

The relational tests use disposable databases. No migration is part of Phase 4.
