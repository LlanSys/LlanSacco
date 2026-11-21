using System;
using System.Threading.Tasks;
using LS.Domain.Features.IAM.Menus.Entities;
using LS.Domain.Shared.Contracts.Common;
using LS.Persistence.Features.IAM.DataContext;
using LS.Tests.Integration.TestFixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LS.Tests.Integration;

public class IamTenantIsolationTests_SqlServer(MsSqlDbFixture fixture) : IamTenantIsolationTests<MsSqlDbFixture>(fixture)
{
}

