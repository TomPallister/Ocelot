using Ocelot.Infrastructure.Extensions;
using Shouldly;
using Xunit;

namespace Ocelot.UnitTests.Infrastructure
{
    public class StringExtensionsTests
    {
        [Fact]
        public void should_trim_start()
        {
            var test = "/string";

            test = test.TrimStart("/");

            test.ShouldBe("string");
        }

        [Fact]
        public void should_return_source()
        {
            var test = "string";

            test = test.LastCharAsForwardSlash();

            test.ShouldBe("string/");
        }
    }
}
