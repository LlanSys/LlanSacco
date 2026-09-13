using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Accounting.Dtos;
using MediatR;
using System;

namespace LS.Application.Features.Accounting.Commands;

public record CreateJournalCommand(CreateJournalRequest Request) : IRequest<AppResponse<Guid>>;
