using LS.Tests.Integration.TestFixtures;
namespace LS.Tests.Integration;

public sealed class Phase2PersistenceTests_PostgreSql(PostgreSqlDbFixture fixture) : Phase2PersistenceTests<PostgreSqlDbFixture>(fixture);
