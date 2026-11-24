using LS.Tests.Integration.TestFixtures;

namespace LS.Tests.Integration;

public sealed class GuarantorReservationTests_SqlServer(MsSqlDbFixture fixture) : GuarantorReservationTests<MsSqlDbFixture>(fixture);
