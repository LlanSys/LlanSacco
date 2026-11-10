using LS.SharedKernel.Features.Shared.Reporting.Dtos;

namespace LS.Application.Features.Shared.Reporting.Contracts.Interfaces;

public interface IPdfReportService
{
    PdfReportResponse Generate(PdfReportRequest request);
}
