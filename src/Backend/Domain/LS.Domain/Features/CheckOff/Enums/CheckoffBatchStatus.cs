namespace LS.Domain.Features.CheckOff.Enums;

public enum CheckoffBatchStatus
{
    Staged,
    Validating,
    Validated,
    Posting,
    Posted,
    Failed
}
