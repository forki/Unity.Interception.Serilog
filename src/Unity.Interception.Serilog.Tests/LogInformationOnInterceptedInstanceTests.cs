using System.Linq;
using FluentAssertions;
using Serilog.Events;
using Unity.Interception.Serilog.Tests.Support;
using Xunit;

namespace Unity.Interception.Serilog.Tests
{
    public class LogInformationOnInterceptedInstanceTests : TestBase
    {

        public LogInformationOnInterceptedInstanceTests()
        {
            GivenThereExistsAContainer()
                .WithConfiguredSerilog(level: LogEventLevel.Information)
                .WithADummyInstanceRegistered();
            WhenDummyIsResolvedAnd().ReturnStuff(1, "b");
        }

        [Fact]
        public void ThenAnInformationMessageShouldBeLogged()
        {
            var entry = Log.Single();
            entry.Level.Should().Be(LogEventLevel.Information);
        }
    }
}