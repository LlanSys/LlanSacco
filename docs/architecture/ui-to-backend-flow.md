# UI-to-Backend Flow

The default product-building flow is:

1. Start from the UI journey.
2. Call backend APIs through typed HTTP clients.
3. Keep DTO contracts in `LS.SharedKernel`.
4. Keep UI state, presentation models, and browser/session concerns in the UI layer.
5. Push business rules into Application and Domain when the journey exposes them.

## Default Rule

Frontend projects must not reference backend Application, Domain, Persistence, EF Core contexts, repositories, or MediatR handlers directly.

Use this boundary:

```text
Blazor UI -> typed HTTP client -> LS.Api -> Application -> Domain -> Persistence/Infrastructure
```

This keeps the API contract real from day one. The same backend endpoints can later serve Blazor, mobile, external API clients, API Management, integration tests, and deployment probes.

## Why Not Call Application Directly?

Direct Application calls can reduce HTTP serialization overhead, but they also couple the UI deployment to backend internals. That coupling makes these things harder:

- Independent API/UI deployment.
- Azure API Management adoption.
- Mobile and external client reuse.
- Authentication and authorization consistency.
- Contract testing.
- Observability at the API boundary.
- Versioning and backward compatibility.

For this base template, the HTTP boundary is the enterprise default.

## Allowed Exception

Direct in-process Application calls are allowed only when a module is explicitly designed as one deployable unit and does not need to expose the same capability to API, mobile, external clients, or API Management.

Document the exception before using it.

## Operation permissions

Check-off uses separate grants for viewing batches, preparing uploads, and posting deductions. Payroll uses separate read, run, close, and settings grants. The corresponding Blazor routes and action visibility use the API permission keys. The accounting integration error page separates viewing failures from retrying them. The API returns a sanitized error summary and keeps provider details in the diagnostic record.

IAM role mappings remain tenant-owned. Apply the provider catalog migrations to populate the default tenant's definitions. For other tenants, register the required context, resource and action in the access catalog, create the matching permission, and grant it to the relevant operator roles. Refresh the operator session after granting access. No existing role receives these new grants through the migration.

The member self-service policy requires an authenticated actor. Dividends and guarantor handlers resolve the active membership linked to that actor inside the current tenant and apply ownership checks before returning data or staging a write.
