# LlanSacco UI Test Guide

This document details the expected UI workflows and manual testing steps for validating the Phase 6 Core UI implementations. Use this guide to methodically test features item by item.

---

## 1. Membership Module

### Member Onboarding
**Path:** `/membership/onboarding`
- **Goal:** Verify that a new member can be successfully created and added to the Pending Approvals queue.
- **Steps:**
  1. Navigate to Member Onboarding.
  2. Fill out all required fields (Name, Email, ID Number, Phone, Date of Birth).
  3. Enter valid address information.
  4. Ensure validation errors appear if required fields are omitted.
  5. Click "Submit".
- **Expected Result:** Success notification appears. The page clears or redirects, and the member is placed in a "Pending" status for approval.

### Member List & Details
**Path:** `/membership` -> `/membership/{id}`
- **Goal:** Verify that existing members can be viewed and their profiles accessed.
- **Steps:**
  1. Navigate to the Membership dashboard.
  2. Ensure the Data Table loads members correctly (compact layout).
  3. Click on a member to view details.
  4. Verify the Profile page displays the correct information and Guarantor lists.
- **Expected Result:** Member details match backend data; tables are responsive and use the correct compact MudBlazor UI styling.

---

## 2. Loans Module

### Loan Products Management (Admin)
**Path:** `/loans/products`
- **Goal:** Verify that administrators can Create and Edit Loan Products.
- **Steps:**
  1. Navigate to Loan Products.
  2. Click "Add Product".
  3. Fill out the dialog (Name, Code, Interest Rate, Interest Method).
  4. Save the product.
  5. Edit an existing product and modify its interest rate.
- **Expected Result:** Products are added and updated successfully; the data table reflects the changes immediately.

### Loan Applications (Admin/Manager Queue)
**Path:** `/loans/applications`
- **Goal:** Verify the approval and rejection workflows using SweetAlert2 dialogs.
- **Steps:**
  1. Navigate to Loan Applications.
  2. Locate an application in "Pending" status.
  3. Click "Approve". Confirm via the SweetAlert2 prompt.
  4. Locate another "Pending" application.
  5. Click "Reject". Ensure the SweetAlert2 prompt requires a rejection reason before allowing submission.
- **Expected Result:** Statuses change correctly to Approved/Rejected, and rejection reasons are captured accurately.

### Apply for Loan (Member Portal)
**Path:** `/loans/apply`
- **Goal:** Verify the member application form with dynamic lists.
- **Steps:**
  1. Navigate to Apply for Loan.
  2. Select a Loan Product and enter the requested amount.
  3. Add two Guarantors by clicking "Add Guarantor" and filling in the Member IDs/Amounts.
  4. Remove one Guarantor.
  5. Submit the application.
- **Expected Result:** The application submits successfully and is visible in the Admin Queue.

---

## 3. Accounting Module

### Trial Balance View
**Path:** `/accounting/trial-balance`
- **Goal:** Verify reporting and data fetching.
- **Steps:**
  1. Navigate to the Trial Balance.
  2. Select an "As Of Date".
  3. Click "Generate Report".
- **Expected Result:** The table populates with active accounts and correct debit/credit balances. The Totals row matches exactly.

### Journal Entry Form
**Path:** `/accounting/journal-entry`
- **Goal:** Verify double-entry accounting logic and validation.
- **Steps:**
  1. Navigate to New Journal Entry.
  2. Enter a reference number and description.
  3. Add a Debit line for 500 to an Asset account.
  4. Attempt to submit. **Expected Result:** The submit button is disabled or an error fires because Debit != Credit.
  5. Add a Credit line for 500 to a Revenue account.
  6. Click "Post Journal". Confirm via the SweetAlert2 prompt.
- **Expected Result:** Journal posts successfully and redirects back to the Accounting dashboard or Trial Balance.

---

## 4. SaaS & IAM Alignment

### Tenant and Styling Verification
- **Goal:** Verify multi-tenant scoped access and branding.
- **Steps:**
  1. Log in as a tenant-specific user.
  2. Observe the color palette (should reflect Palette P1: Royal Blue).
  3. Inspect the local storage/session storage to ensure tokens are saved properly and securely.
  4. Refresh the page to verify session persistence.
- **Expected Result:** The user remains logged in, the UI maintains the correct theme, and data retrieved is scoped solely to that tenant.
