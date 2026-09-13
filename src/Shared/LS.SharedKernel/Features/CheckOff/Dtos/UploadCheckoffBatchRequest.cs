using System;
using System.Collections.Generic;


namespace LS.SharedKernel.Features.CheckOff.Dtos;

public class UploadCheckoffBatchRequest
{
    public Guid EmployerId { get; set; }
    public string BatchReference { get; set; } = string.Empty;
    public DateTime ProcessingPeriod { get; set; } = DateTime.UtcNow;
    public List<CheckoffRowRequest> Rows { get; set; } = new();
}

