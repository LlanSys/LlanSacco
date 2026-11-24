<#
.SYNOPSIS
    Seeds the FICTIONAL "Jamii Reference SACCO" data set used for the signed-in
    browser acceptance of the Phase 1 guarantor flow.

.DESCRIPTION
    Purpose
      Drives the running LlanSacco API to create the staff roles/users, members,
      savings product, deposits, loan product and one guaranteed loan application
      described in:
        - docs/architecture/reference-sacco-default-decisions.md
        - docs/architecture/reference-sacco-operating-profile.md (section 4)
        - docs/architecture/production-readiness-phase-1-verification.md
      After a successful run, guarantors G1 and G2 each have a Nominated pledge
      that they can accept on /loans/my-guarantees. Guarantor consent, credit
      approval and disbursement are deliberately left to the browser session.

    FICTIONAL DATA ONLY
      Every person, e-mail (@jamii.example), national ID and amount here is made
      up. NEVER run this script against staging with real members or against
      production. It refuses to run unless the API host is localhost/127.0.0.1
      or -AllowNonLocalHost is given explicitly.

    Prerequisites
      - PowerShell 7+.
      - The API running in Development (default http://localhost:8090) with
        PostgreSQL, messaging and migrations applied, and the development
        identity seed enabled (admin aamodhiambo@gmail.com, "System Administrator").
      - The admin password (pass as SecureString; it is never printed or stored).
      - If the admin has TOTP enabled, a current 6-digit code (-AdminTotpCode or prompt).
        If MFA enrollment is enforced and the admin has NOT enrolled, every admin
        call returns 403 MFA_ENROLLMENT_REQUIRED: enroll at /iam/security first.

    Known gap: activation of new IAM users
      POST api/v1/iam/admin/users creates users with IsActive = false and
      EmailConfirmed = false, and the login handler rejects both. No API activates
      a non-employee user. The -ActivateUsersInLocalDatabase switch flips those two
      flags for *@jamii.example users only, directly in the local Docker
      PostgreSQL (ops/local/docker-compose.yml, service "postgres"). Without it the
      script stops before member approval (which needs a second, signed-in actor
      because of maker-checker) and tells you what is left.

    Generated passwords
      Staff and member passwords are generated here and written ONLY to
      $env:TEMP\llansacco-acceptance-credentials.json (ops/local/ is not
      git-ignored in this repository). They are never printed. Re-runs reuse the
      passwords in that file.

    Idempotency
      Roles by name, users by e-mail, members by national ID, savings product by
      code, deposits by ExternalReferenceId (server-side replay), loan product by
      code, loan application by borrower + principal + non-terminal status.

.EXAMPLE
    $pw = Read-Host -AsSecureString 'Admin password'
    ./scripts/seed-reference-acceptance.ps1 -AdminPassword $pw -ActivateUsersInLocalDatabase
#>
[CmdletBinding()]
param(
    [string] $ApiBaseUrl = 'http://localhost:8090',
    [Guid]   $TenantId = '0194f700-0000-7000-8000-000000000001',
    [string] $AdminUserName = 'aamodhiambo@gmail.com',
    [Parameter(Mandatory)] [SecureString] $AdminPassword,
    [string] $AdminTotpCode,
    [switch] $ActivateUsersInLocalDatabase,
    [string] $ComposeFile = (Join-Path $PSScriptRoot '..\ops\local\docker-compose.yml'),
    [string] $CredentialsPath = (Join-Path $env:TEMP 'llansacco-acceptance-credentials.json'),
    [switch] $AllowNonLocalHost
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ApiBaseUrl = $ApiBaseUrl.TrimEnd('/')
$apiHost = ([Uri]$ApiBaseUrl).Host
if (-not $AllowNonLocalHost -and $apiHost -notin @('localhost', '127.0.0.1', '::1')) {
    throw "Refusing to seed fictional data into '$apiHost'. Use a local API or pass -AllowNonLocalHost deliberately."
}

# ---------------------------------------------------------------- reference data
$Policy = @{
    EvidenceReference = 'OP-04 reference default'
    SavingsCode       = 'JNWD'
    SavingsName       = 'Jamii Non-Withdrawable Deposits'
    LoanCode          = 'JDL'
    LoanName          = 'Jamii Development Loan'
    Principal         = 200000
    TermMonths        = 24
    GuaranteeEach     = 100000
}

$RoleDefinitions = [ordered]@{
    'Jamii Credit Maker'       = @('applications.view', 'applications.create', 'applications.approve')
    'Jamii Credit Checker'     = @('applications.view', 'applications.approve', 'applications.disburse')
    'Jamii Banking Reconciler' = @('applications.view', 'banking-accounts.view', 'banking-products.manage',
                                   'banking-products.view', 'banking-transactions.post')
    'Jamii Member Officer'     = @('members.create', 'members.approve', 'members.view')
}
$MemberRole = 'Customer (Member)'   # Roles.Customer display name, seeded by DevelopmentIdentitySeeder

$StaffDefinitions = @(
    @{ Key = 'CreditMaker';   Email = 'credit.maker@jamii.example';      First = 'Neema';  Last = 'Maker';      IdNumber = '91000001'; Phone = '+254700910001'; Gender = 'Female'; Role = 'Jamii Credit Maker' }
    @{ Key = 'CreditChecker'; Email = 'credit.checker@jamii.example';    First = 'Otieno'; Last = 'Checker';    IdNumber = '91000002'; Phone = '+254700910002'; Gender = 'Male';   Role = 'Jamii Credit Checker' }
    @{ Key = 'Reconciler';    Email = 'banking.reconciler@jamii.example'; First = 'Halima'; Last = 'Reconciler'; IdNumber = '91000003'; Phone = '+254700910003'; Gender = 'Female'; Role = 'Jamii Banking Reconciler' }
    @{ Key = 'MemberOfficer'; Email = 'member.officer@jamii.example';    First = 'Kamau';  Last = 'Officer';    IdNumber = '91000004'; Phone = '+254700910004'; Gender = 'Male';   Role = 'Jamii Member Officer' }
)

# National IDs avoid "00000000" (SimulatedKycOrchestrator rejects it) and the dev seed IDs.
$MemberDefinitions = @(
    @{ Key = 'B';  Email = 'amani.borrower@jamii.example';   First = 'Amani';  Last = 'Mwangi';  IdNumber = '90000001'; Phone = '+254700900001'; Gender = 'Female'; Dob = '1985-03-14'; Deposit = 200000 }
    @{ Key = 'G1'; Email = 'baraka.guarantor@jamii.example'; First = 'Baraka'; Last = 'Otieno';  IdNumber = '90000002'; Phone = '+254700900002'; Gender = 'Male';   Dob = '1980-07-02'; Deposit = 200000 }
    @{ Key = 'G2'; Email = 'chebet.guarantor@jamii.example'; First = 'Chebet'; Last = 'Kiprono'; IdNumber = '90000003'; Phone = '+254700900003'; Gender = 'Female'; Dob = '1988-11-21'; Deposit = 200000 }
    @{ Key = 'X';  Email = 'dalia.other@jamii.example';      First = 'Dalia';  Last = 'Njeri';   IdNumber = '90000004'; Phone = '+254700900004'; Gender = 'Female'; Dob = '1992-01-09'; Deposit = 50000 }
)

$Summary = [System.Collections.Generic.List[object]]::new()
function Add-Summary([string] $Kind, [string] $Name, [string] $Id, [string] $Note = '') {
    $Summary.Add([pscustomobject]@{ Kind = $Kind; Name = $Name; Id = $Id; Note = $Note })
}

# ---------------------------------------------------------------- HTTP helpers
function Invoke-Api {
    param(
        [Parameter(Mandatory)] [ValidateSet('GET', 'POST', 'PUT', 'PATCH', 'DELETE')] [string] $Method,
        [Parameter(Mandatory)] [string] $Path,
        [object] $Body,
        [string] $Token,
        [int[]] $AllowStatus = @()
    )
    $headers = @{ 'X-Tenant-Id' = $TenantId.ToString(); Accept = 'application/json' }
    if ($Token) { $headers.Authorization = "Bearer $Token" }
    $params = @{
        Method = $Method; Uri = "$ApiBaseUrl/$($Path.TrimStart('/'))"; Headers = $headers
        SkipHttpErrorCheck = $true; ContentType = 'application/json'
    }
    if ($null -ne $Body) { $params.Body = ($Body | ConvertTo-Json -Depth 10 -Compress) }

    $response = Invoke-WebRequest @params
    $status = [int]$response.StatusCode
    $parsed = $null
    # application/problem+json bodies arrive as bytes; decode before parsing.
    $content = if ($response.Content -is [byte[]]) { [Text.Encoding]::UTF8.GetString($response.Content) } else { $response.Content }
    if ($content) { try { $parsed = $content | ConvertFrom-Json -Depth 20 } catch { $parsed = $content } }

    if ($status -ge 200 -and $status -lt 300) {
        if ($parsed -is [psobject] -and $parsed.PSObject.Properties['data']) { return $parsed.data }
        return $parsed
    }
    if ($status -in $AllowStatus) { return [pscustomobject]@{ __status = $status; __body = $parsed } }

    $detail = if ($parsed -is [string]) { $parsed } else { $parsed | ConvertTo-Json -Depth 10 -Compress }
    throw "API $Method $Path failed with HTTP $status. Body: $detail"
}

function Test-Failed($result) { $null -ne $result -and $result -is [psobject] -and $result.PSObject.Properties['__status'] }

function Connect-Api {
    param([string] $UserName, [string] $PlainPassword, [string] $TotpCode, [switch] $AllowTotpPrompt)
    $login = Invoke-Api POST 'api/v1/iam/auth/login' -Body @{
        userName = $UserName; password = $PlainPassword; rememberMe = $false; returnUrl = $null
        deviceFingerprint = 'seed-reference-acceptance'
    } -AllowStatus 400, 401, 403, 422
    if (Test-Failed $login) {
        $msg = $login.__body | ConvertTo-Json -Depth 5 -Compress
        throw "Sign-in failed for $UserName (HTTP $($login.__status)): $msg"
    }
    if ($login.requires2FA) {
        if (-not $TotpCode -and $AllowTotpPrompt) { $TotpCode = Read-Host "TOTP code for $UserName" }
        if (-not $TotpCode) { throw "$UserName requires two-factor authentication; pass -AdminTotpCode." }
        $verified = Invoke-Api POST 'api/v1/iam/users/totp/verify' -Body @{
            userId = $login.userId; code = $TotpCode; rememberMe = $false
        }
        $token = if ($verified.PSObject.Properties['token']) { $verified.token } else { $null }
        if (-not $token) { throw "TOTP verification for $UserName did not return an access token." }
        return $token
    }
    if ($login.mfaEnrollmentRequired) {
        Write-Warning "$UserName must enroll in MFA; protected calls may return 403 MFA_ENROLLMENT_REQUIRED (enroll at /iam/security)."
    }
    return $login.token
}

function New-StrongPassword {
    $sets = @('ABCDEFGHJKLMNPQRSTUVWXYZ', 'abcdefghijkmnopqrstuvwxyz', '23456789', '!@#$%^&*-_=+?')
    $chars = foreach ($s in $sets) { $s[[System.Security.Cryptography.RandomNumberGenerator]::GetInt32($s.Length)] }
    $all = -join $sets
    $chars += 1..16 | ForEach-Object { $all[[System.Security.Cryptography.RandomNumberGenerator]::GetInt32($all.Length)] }
    -join ($chars | Sort-Object { [System.Security.Cryptography.RandomNumberGenerator]::GetInt32([int]::MaxValue) })
}

# ---------------------------------------------------------------- credentials file
$credentials = @{}
if (Test-Path $CredentialsPath) {
    (Get-Content $CredentialsPath -Raw | ConvertFrom-Json -AsHashtable).GetEnumerator() |
        ForEach-Object { $credentials[$_.Key] = $_.Value }
}
function Save-Credentials {
    $credentials | ConvertTo-Json -Depth 5 | Set-Content -Path $CredentialsPath -Encoding utf8NoBOM
}

# ---------------------------------------------------------------- 1. admin sign-in
Write-Host "Signing in as admin against $ApiBaseUrl (tenant $TenantId)..."
$adminPlain = ConvertFrom-SecureString -SecureString $AdminPassword -AsPlainText
$adminToken = Connect-Api -UserName $AdminUserName -PlainPassword $adminPlain -TotpCode $AdminTotpCode -AllowTotpPrompt
$adminPlain = $null

# ---------------------------------------------------------------- 2. roles + permissions
Write-Host 'Ensuring staff roles and permissions...'
$roles = @(Invoke-Api GET 'api/v1/iam/admin/roles' -Token $adminToken)
foreach ($roleName in $RoleDefinitions.Keys) {
    $role = $roles | Where-Object name -EQ $roleName | Select-Object -First 1
    if (-not $role) {
        $role = Invoke-Api POST 'api/v1/iam/admin/roles' -Token $adminToken -Body @{ name = $roleName; departmentId = $null }
    }
    # PUT replaces the role's permission set, so re-running converges to the definition.
    $null = Invoke-Api PUT "api/v1/iam/admin/roles/$($role.id)/permissions" -Token $adminToken -Body @{ permissionKeys = $RoleDefinitions[$roleName] }
    Add-Summary 'Role' $roleName $role.id ($RoleDefinitions[$roleName] -join ', ')
}

# ---------------------------------------------------------------- 3. IAM users
function Find-User([string] $Email) {
    $page = Invoke-Api GET "api/v1/iam/admin/users?GlobalSearch=$([Uri]::EscapeDataString($Email))&PageSize=50" -Token $adminToken
    @($page.items) | Where-Object { $_.email -eq $Email } | Select-Object -First 1
}

function Initialize-User([hashtable] $Def, [string[]] $Roles) {
    $user = Find-User $Def.Email
    if (-not $user) {
        if (-not $credentials.ContainsKey($Def.Email)) { $credentials[$Def.Email] = New-StrongPassword; Save-Credentials }
        $created = Invoke-Api POST 'api/v1/iam/admin/users' -Token $adminToken -AllowStatus 409 -Body @{
            username = $Def.Email; email = $Def.Email; password = $credentials[$Def.Email]
            firstName = $Def.First; lastName = $Def.Last; phoneNumber = $Def.Phone
            idNumber = $Def.IdNumber; gender = $Def.Gender; employeeId = $null; memberId = $null; roles = $Roles
        }
        # The admin user list is a cached query, so prefer the create response.
        $user = if (-not (Test-Failed $created) -and $created.id) {
            [pscustomobject]@{ id = $created.id; email = $Def.Email; roles = $Roles; isActive = $false; emailConfirmed = $false; memberId = $null }
        } else { Find-User $Def.Email }
        if (-not $user) { throw "User $($Def.Email) could not be created or found (create returned $($created | ConvertTo-Json -Compress))." }
    } elseif (-not $credentials.ContainsKey($Def.Email)) {
        Write-Warning "$($Def.Email) already exists but its password is not in $CredentialsPath; reset it via admin if you need to sign in."
    }
    $missing = @($Roles | Where-Object { $_ -notin @($user.roles) })
    if ($missing.Count -gt 0) {
        $null = Invoke-Api PUT "api/v1/iam/admin/users/$($user.id)/roles" -Token $adminToken -Body @{ roles = @(@($user.roles) + $missing | Select-Object -Unique) }
    }
    return $user
}

Write-Host 'Ensuring staff users...'
$staffUsers = @{}
foreach ($s in $StaffDefinitions) {
    $u = Initialize-User $s @($s.Role)
    $staffUsers[$s.Key] = $u
    Add-Summary 'Staff user' $s.Email $u.id $s.Role
}

Write-Host 'Ensuring member IAM users (national ID drives member linkage at onboarding)...'
$memberUsers = @{}
foreach ($m in $MemberDefinitions) {
    $memberUsers[$m.Key] = Initialize-User $m @($MemberRole)
}

# ---------------------------------------------------------------- 4. activation gap
$jamiiEmails = @($StaffDefinitions + $MemberDefinitions | ForEach-Object { $_.Email })
$inactive = @($jamiiEmails | ForEach-Object { Find-User $_ } | Where-Object { -not $_.isActive -or -not $_.emailConfirmed })
if ($inactive.Count -gt 0) {
    if ($ActivateUsersInLocalDatabase) {
        Write-Host "Activating $($inactive.Count) @jamii.example users in the local Docker PostgreSQL (development-only gap fill)..."
        $sql = @'
UPDATE "Users"
   SET "IsActive" = TRUE, "EmailConfirmed" = TRUE, "ActivatedAt" = COALESCE("ActivatedAt", now()),
       "DeactivatedAt" = NULL, "DeactivatedBy" = NULL, "DeactivationReason" = NULL
 WHERE "Email" LIKE '%@jamii.example' AND ("IsActive" = FALSE OR "EmailConfirmed" = FALSE);
'@
        $sql | docker compose -f $ComposeFile exec -T postgres sh -c 'psql -v ON_ERROR_STOP=1 -U "$POSTGRES_USER" -d llansacco'
        if ($LASTEXITCODE -ne 0) { throw 'Local database activation failed (docker compose exec postgres psql).' }
    } else {
        Write-Warning ("These users are inactive or unconfirmed and cannot sign in: " + (($inactive | ForEach-Object email) -join ', '))
        Write-Warning 'The API has no activation endpoint for non-employee users. Re-run with -ActivateUsersInLocalDatabase (local Docker only).'
    }
}

# ---------------------------------------------------------------- 5. members (onboard as admin = maker)
Write-Host 'Ensuring members...'
$allMembers = @(Invoke-Api GET 'api/v1/members' -Token $adminToken)
$members = @{}
foreach ($m in $MemberDefinitions) {
    $member = $allMembers | Where-Object identificationNumber -EQ $m.IdNumber | Select-Object -First 1
    if (-not $member) {
        $id = Invoke-Api POST 'api/v1/members/onboard' -Token $adminToken -Body @{
            firstName = $m.First; lastName = $m.Last; email = $m.Email; phoneNumber = $m.Phone
            identificationNumber = $m.IdNumber; dateOfBirth = $m.Dob; membershipType = 'Individual'
        }
        $member = Invoke-Api GET "api/v1/members/$id" -Token $adminToken
    }
    if ($member.kycStatus -ne 'Verified') { throw "Member $($m.Key) ($($member.id)) has KYC status '$($member.kycStatus)'; expected Verified." }
    $members[$m.Key] = $member
}

# Approval must be a different actor (ApproveMember enforces maker != checker).
$pending = @($members.Values | Where-Object status -EQ 'PendingApproval')
if ($pending.Count -gt 0) {
    $officer = $StaffDefinitions | Where-Object Key -EQ 'MemberOfficer'
    if (-not $credentials.ContainsKey($officer.Email)) { throw "No stored password for $($officer.Email); cannot approve members as an independent checker." }
    try {
        $officerToken = Connect-Api -UserName $officer.Email -PlainPassword $credentials[$officer.Email]
    } catch {
        throw "Cannot sign in as $($officer.Email) to approve members ($($_.Exception.Message)). Activate users (-ActivateUsersInLocalDatabase) and re-run."
    }
    foreach ($p in $pending) {
        $null = Invoke-Api POST 'api/v1/members/approve' -Token $officerToken -Body @{ memberId = $p.id; approvalNote = 'Jamii reference acceptance seed (fictional)' }
    }
}
foreach ($m in $MemberDefinitions) {
    $member = Invoke-Api GET "api/v1/members/$($members[$m.Key].id)" -Token $adminToken
    $members[$m.Key] = $member
    $linked = (Find-User $m.Email).memberId
    $note = "status=$($member.status) kyc=$($member.kycStatus) iamUser=$($memberUsers[$m.Key].id)"
    if ($linked -ne $member.id) { $note += ' WARNING: IAM user not linked to this member' }
    Add-Summary "Member $($m.Key)" "$($m.First) $($m.Last) <$($m.Email)>" $member.id $note
}

# ---------------------------------------------------------------- 6. savings product + security policy
Write-Host 'Ensuring savings product and guarantor eligibility...'
$savingsProduct = @(Invoke-Api GET 'api/banking/savings-products?onlyActive=false' -Token $adminToken) |
    Where-Object code -EQ $Policy.SavingsCode | Select-Object -First 1
if (-not $savingsProduct) {
    $savingsProduct = Invoke-Api POST 'api/banking/savings-products' -Token $adminToken -Body @{
        name = $Policy.SavingsName; code = $Policy.SavingsCode
        description = 'Fictional reference non-withdrawable member deposits (BOSA).'
        interestRate = 0; minimumBalance = 0; allowsWithdrawals = $false; withdrawalFee = 0
        dailyWithdrawalLimit = $null; monthlyWithdrawalLimit = $null
    }
}
$null = Invoke-Api PUT "api/banking/savings-products/$($savingsProduct.id)/security-policy" -Token $adminToken -Body @{
    allowsGuarantorReservations = $true; countsTowardLoanLimit = $true; evidenceReference = $Policy.EvidenceReference
}
Add-Summary 'Savings product' $Policy.SavingsName $savingsProduct.id 'guarantor reservations + loan limit enabled'

# ---------------------------------------------------------------- 7. deposits (ExternalReferenceId makes replays safe)
Write-Host 'Posting deposits...'
$savingsAccounts = @{}
foreach ($m in $MemberDefinitions) {
    $account = Invoke-Api POST 'api/banking/savings/deposit' -Token $adminToken -Body @{
        memberId = $members[$m.Key].id; savingsProductId = $savingsProduct.id; amount = $m.Deposit
        notes = 'Jamii reference acceptance opening deposit (fictional)'
        externalReferenceId = "JAMII-REF-DEP-$($m.Key)-001"
    }
    $savingsAccounts[$m.Key] = $account
    Add-Summary "Savings acct $($m.Key)" $Policy.SavingsName $account.id "balance=$($account.balance) locked=$($account.lockedFunds)"
}

# ---------------------------------------------------------------- 8. loan product
Write-Host 'Ensuring loan product...'
$loanProduct = @(Invoke-Api GET 'api/v1/loans/products' -Token $adminToken) | Where-Object productCode -EQ $Policy.LoanCode | Select-Object -First 1
if (-not $loanProduct) {
    # GuarantorPolicyVersion is omitted on purpose so the product default applies.
    $loanProductId = Invoke-Api POST 'api/v1/loans/products' -Token $adminToken -Body @{
        productCode = $Policy.LoanCode; productName = $Policy.LoanName
        description = 'Fictional reference product: 12% reducing, up to 36 months, KES 1,000,000.'
        interestRate = 12; interestMethod = 'ReducingBalance'; maxTermInMonths = 36; maxAmount = 1000000
        eligibleDepositMultiple = 3; guarantorReservationDays = 30; guarantorConsentDays = 14
    }
    $loanProduct = @(Invoke-Api GET 'api/v1/loans/products' -Token $adminToken) | Where-Object id -EQ $loanProductId | Select-Object -First 1
}
Add-Summary 'Loan product' $Policy.LoanName $loanProduct.id "policy=$($loanProduct.guarantorPolicyVersion) consentDays=$($loanProduct.guarantorConsentDays)"

# ---------------------------------------------------------------- 9. loan application with two guarantors
Write-Host 'Ensuring loan application for B...'
$terminal = @('Rejected', 'Cancelled', 'Closed', 'Settled', 'WrittenOff')
$existingLoan = @(Invoke-Api GET "api/v1/loans/applications?memberId=$($members['B'].id)" -Token $adminToken) |
    Where-Object { $_.loanProductId -eq $loanProduct.id -and [decimal]$_.principalAmount -eq $Policy.Principal -and $_.status -notin $terminal } |
    Select-Object -First 1
if ($existingLoan) {
    $loanId = $existingLoan.id
} else {
    $loanId = Invoke-Api POST 'api/v1/loans/applications' -Token $adminToken -Body @{
        memberId = $members['B'].id; loanProductId = $loanProduct.id
        principalAmount = $Policy.Principal; termInMonths = $Policy.TermMonths
        guarantors = @(
            @{ guarantorMemberId = $members['G1'].id; guaranteedAmount = $Policy.GuaranteeEach }
            @{ guarantorMemberId = $members['G2'].id; guaranteedAmount = $Policy.GuaranteeEach }
        )
    }
}
$loan = @(Invoke-Api GET "api/v1/loans/applications?memberId=$($members['B'].id)" -Token $adminToken) | Where-Object id -EQ $loanId | Select-Object -First 1
Add-Summary 'Loan application' "$($loan.applicationNumber)" $loanId "status=$($loan.status) principal=$($loan.principalAmount) term=$($loan.termInMonths)"

# ---------------------------------------------------------------- summary
Write-Host ''
Write-Host 'Jamii Reference SACCO seed (FICTIONAL data) - summary'
$Summary | Format-Table -AutoSize -Wrap | Out-String -Width 220 | Write-Host
Write-Host "Credentials (generated passwords, not printed): $CredentialsPath"
Write-Host 'Next, in the browser: G1 and G2 sign in and accept on /loans/my-guarantees (pledge, account, exact terms version);'
Write-Host 'X confirms it cannot act on G1''s pledge; credit maker/checker review /loans/applications/{loanId}/guarantees;'
Write-Host 'reconciler checks /loans/guarantor-reconciliation. Consent must happen within the product consent window (14 days).'
Write-Host 'Not seeded: GUARANTOR_RECOVERY GL mapping (no API exists; recovery journeys will raise a missing-mapping exception).'
