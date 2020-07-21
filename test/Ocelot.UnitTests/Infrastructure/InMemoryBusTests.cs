using Ocelot.Infrastructure;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Ocelot.UnitTests.Infrastructure
{
    public class InMemoryBusTests
    {
        private readonly InMemoryBus<object> _bus;

        public InMemoryBusTests()
        {
            _bus = new InMemoryBus<object>();
        }

        [Fact]
        public async Task should_publish_with_delay()
        {
            var called = false;
            _bus.Subscribe(x =>
            {
                called = true;
            });
            _bus.Publish(new object(), 1);
            await Task.Delay(100);
            called.ShouldBeTrue();
        }

        [Fact]
        public void should_not_be_publish_yet_as_no_delay_in_caller()
        {
            var called = false;
            _bus.Subscribe(x =>
            {
                called = true;
            });
            _bus.Publish(new object(), 1);
            called.ShouldBeFalse();
        }
    }
}
