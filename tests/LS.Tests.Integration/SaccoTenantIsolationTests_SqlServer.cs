using LS.Tests.Integration.TestFixtures;

namespace LS.Tests.Integration;

public sealed class SaccoTenantIsolationTests_SqlServer(MsSqlDbFixture fixture) : SaccoTenantIsolationTests<MsSqlDbFixture>(fixture);
