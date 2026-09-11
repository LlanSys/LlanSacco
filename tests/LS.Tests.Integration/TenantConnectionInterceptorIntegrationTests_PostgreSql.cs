using LS.Domain.Shared.Contracts.Common;
using LS.Persistence.Common.Interceptors;
using LS.Persistence.Features.Shared.DataContext;
using LS.Tests.Integration.TestFixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace LS.Tests.Integration;

public class TenantConnectionInterceptorIntegrationTests_PostgreSql(PostgreSqlDbFixture fixture) : TenantConnectionInterceptorIntegrationTests<PostgreSqlDbFixture>(fixture)
{
}

