using LS.Api.Common.Controllers;
using LS.Application.Features.CheckOff.Commands.Employers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace LS.Api.Features.CheckOff.Controllers;

public record LinkMemberRequest(
    string EmployeePayrollNumber,
    decimal BasicSalary,
    DateTime DateEmployed);
