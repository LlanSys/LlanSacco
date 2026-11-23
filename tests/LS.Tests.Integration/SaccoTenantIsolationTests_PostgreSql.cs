using LS.Tests.Integration.TestFixtures;

namespace LS.Tests.Integration;

public sealed class SaccoTenantIsolationTests_PostgreSql(PostgreSqlDbFixture fixture) : SaccoTenantIsolationTests<PostgreSqlDbFixture>(fixture);
