using Oculus.Platform;
using Xunit;
using Xunit.Abstractions;

namespace Gamer.Estate.Tes.Tests
{
    public class PlatformCoreTests
    {
        [Fact]
        public void Initialize()
        {
            // given
            Core.Initialize();
            // then
            Assert.True(Core.IsInitialized());
        }
    }
}
