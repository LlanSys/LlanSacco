## OnBoarding

NationalId presented
│
▼
IIdentityResolutionService.FindByNationalIdAsync()
│
┌────┴─────────────────────────────────────────────┐
│ │
Not found Found (AppUser exists)
│ │
▼ ┌────┴──────────────────┐
Create new AppUser Has EmployeeId? Has CustomerId?

- Create Employee/Customer │ │
- Link Yes → already Yes → already
  an employee a customer
  (guard/error) (guard/error)
  │ │
  No No
  ▼ ▼
  LinkToEmployee() LinkToCustomer()
  (no new AppUser) (no new AppUser)

---

## The Full Picture in One Diagram

ONBOARDING FLOW ACTIVE SESSION FLOW
═══════════════ ══════════════════
NationalId presented User logs in, JWT issued
│ │
▼ ▼
IIdentityResolutionService IUserContextService
.FindByNationalIdAsync() .GetCurrentContext()
(queries AspNetUsers table) (reads JWT claims)
│ │
▼ ▼
UserIdentityContext UserIdentityContext
(DB-sourced snapshot) (claims-sourced snapshot)
│ │
▼ ▼
Branch: create or link AuthorizeView / component logic
│ context switching / route guards
▼
IKycOrchestrator.RunXxxKycAsync()
│
┌────┼────┐────────┐
▼ ▼ ▼ ▼
IPRS KRA CRB AML
(each its own adapter,
each its own interface,
mock in dev/test)

---

## Kenya-specific verification stack:

- IPRS (Integrated Population Registration System) — national ID/passport validation

- KRA iTax API — PIN certificate verification

- NTSA — driving licence (secondary)

- TransUnion/Metropol/CreditInfo — CRB checks

- SWIFT Compliance Analytics / Dow Jones — AML/sanctions screening
