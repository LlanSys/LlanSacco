using System.ComponentModel;

namespace LS.Domain.Features.Banking.FOSA.Enums;

public enum OtcTransactionType
{
    [Description("Cash Deposit")]
    CashDeposit = 1,
    
    [Description("Cash Withdrawal")]
    CashWithdrawal = 2
}
