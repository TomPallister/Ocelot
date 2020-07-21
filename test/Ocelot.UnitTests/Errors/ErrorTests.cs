using Ocelot.Infrastructure.RequestData;
using Shouldly;
using Xunit;

namespace Ocelot.UnitTests.Errors
{
    public class ErrorTests
    {
        [Fact]
        public void should_return_message()
        {
            var error = new CannotAddDataError("message");
            var result = error.ToString();
            result.ShouldBe("message");
        }
    }
}
