using LS.Domain.Features.Shared.OrgSettings.Entities;
using LS.Domain.Shared.Contracts.Common;
using LS.Persistence.Features.Shared.DataContext;
using LS.Tests.Integration.TestFixtures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace LS.Tests.Integration;

public class SharedTenantIsolationTests_SqlServer(MsSqlDbFixture fixture) : SharedTenantIsolationTests<MsSqlDbFixture>(fixture)
{
}

