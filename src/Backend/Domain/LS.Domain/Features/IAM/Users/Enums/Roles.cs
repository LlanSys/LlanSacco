using System.ComponentModel;

namespace LS.Domain.Features.IAM.Users.Enums;

public enum Roles
{
    [Description("System Administrator")]
    SysAdmin,

    [Description("Employee (General)")]
    Employee,

    [Description("Customer (Member)")]
    Customer,

    [Description("Teller")]
    Teller,

    [Description("Branch Manager")]
    BranchManager,

    [Description("Credit Officer")]
    CreditOfficer,

    [Description("Credit Committee Member")]
    CreditCommittee,

    [Description("Accountant")]
    Accountant,

    [Description("Internal Auditor")]
    Auditor,

    [Description("Compliance Officer")]
    ComplianceOfficer,

    [Description("Operations Manager")]
    OperationsManager,

    [Description("Treasurer")]
    Treasurer,

    [Description("Board Member")]
    BoardMember,

    [Description("Loan Disbursement Officer")]
    LoanDisbursementOfficer,

    [Description("Collections Officer")]
    CollectionsOfficer,

    [Description("Membership Officer")]
    MembershipOfficer,

    [Description("FOSA Officer")]
    FosaOfficer
}
