using System.ComponentModel;

namespace LS.Domain.Features.Banking.FOSA.Enums;

public enum TillStatus
{
    [Description("Closed")]
    Closed = 0,
    
    [Description("Open")]
    Open = 1,
    
    [Description("Suspended")]
    Suspended = 2
}
