using System;
using System.Collections.Generic;


namespace LS.SharedKernel.Features.CheckOff.Dtos;

public record CheckoffBatchDetailsResponse(
    CheckoffBatchResponse Batch,
    List<CheckoffStagingRowResponse> Rows
);

