using LS.Tests.Integration.TestFixtures;
namespace LS.Tests.Integration;

public sealed class Phase2PersistenceTests_SqlServer(MsSqlDbFixture fixture) : Phase2PersistenceTests<MsSqlDbFixture>(fixture);
