using LS.Domain.Features.IAM.Menus.Entities;
using LS.Domain.Shared.Contracts.Common;
using LS.Persistence.Features.IAM.DataContext;
using LS.Tests.Integration.TestFixtures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace LS.Tests.Integration;

public class TenantIsolationMutationTests_PostgreSql(PostgreSqlDbFixture fixture) : TenantIsolationMutationTests<PostgreSqlDbFixture>(fixture)
{
}

