using LS.Domain.Features.HR.Departments.Entities;
using LS.Domain.Shared.Contracts.Common;
using LS.Persistence.Features.HR.DataContext;
using LS.Tests.Integration.TestFixtures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace LS.Tests.Integration;

public class HrTenantIsolationTests_Postgres(PostgreSqlDbFixture fixture) : HrTenantIsolationTests<PostgreSqlDbFixture>(fixture)
{
}

