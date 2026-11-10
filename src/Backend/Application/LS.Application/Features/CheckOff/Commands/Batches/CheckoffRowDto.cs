using System;

namespace LS.Application.Features.CheckOff.Commands.Batches;

public record CheckoffRowDto(string EmployeePayrollNumber, string MemberName, decimal Amount, string ReferenceNumber);

