namespace Ocelot.UnitTests.LoadBalancer
{
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Ocelot.Infrastructure;
    using Ocelot.LoadBalancer.LoadBalancers;
    using Ocelot.Middleware;
    using Ocelot.Responses;
    using Ocelot.UnitTests.Responder;
    using Ocelot.Values;
    using Shouldly;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TestStack.BDDfy;
    using Xunit;

    public class CookieStickySessionsTests
    {
        private readonly CookieStickySessions _stickySessions;
        private readonly Mock<ILoadBalancer> _loadBalancer;
        private readonly int _defaultExpiryInMs;
        private Response<ServiceHostAndPort> _result;
        private Response<ServiceHostAndPort> _firstHostAndPort;
        private Response<ServiceHostAndPort> _secondHostAndPort;
        private readonly FakeBus<StickySession> _bus;
        private HttpContext _httpContext;

        public CookieStickySessionsTests()
        {
            _httpContext = new DefaultHttpContext();
            _bus = new FakeBus<StickySession>();
            _loadBalancer = new Mock<ILoadBalancer>();
            _defaultExpiryInMs = 0;
            _stickySessions = new CookieStickySessions(_loadBalancer.Object, "sessionid", _defaultExpiryInMs, _bus);
        }

        [Fact]
        public void should_expire_sticky_session()
        {
            this.Given(_ => GivenTheLoadBalancerReturns())
                .And(_ => GivenTheDownstreamRequestHasSessionId("321"))
                .And(_ => GivenIHackAMessageInWithAPastExpiry())
                .And(_ => WhenILease())
                .When(_ => WhenTheMessagesAreProcessed())
                .Then(_ => ThenTheLoadBalancerIsCalled())
                .BDDfy();
        }

        [Fact]
        public void should_return_host_and_port()
        {
            this.Given(_ => GivenTheLoadBalancerReturns())
                .When(_ => WhenILease())
                .Then(_ => ThenTheHostAndPortIsNotNull())
                .BDDfy();
        }

        [Fact]
        public void should_return_same_host_and_port()
        {
            this.Given(_ => GivenTheLoadBalancerReturnsSequence())
                .And(_ => GivenTheDownstreamRequestHasSessionId("321"))
                .When(_ => WhenILeaseTwiceInARow())
                .Then(_ => ThenTheFirstAndSecondResponseAreTheSame())
                .And(_ => ThenTheStickySessionWillTimeout())
                .BDDfy();
        }

        [Fact]
        public void should_return_different_host_and_port_if_load_balancer_does()
        {
            this.Given(_ => GivenTheLoadBalancerReturnsSequence())
                .When(_ => WhenIMakeTwoRequetsWithDifferentSessionValues())
                .Then(_ => ThenADifferentHostAndPortIsReturned())
                .BDDfy();
        }

        [Fact]
        public void should_return_error()
        {
            this.Given(_ => GivenTheLoadBalancerReturnsError())
                .When(_ => WhenILease())
                .Then(_ => ThenAnErrorIsReturned())
                .BDDfy();
        }

        [Fact]
        public void should_release()
        {
            _stickySessions.Release(new ServiceHostAndPort("", 0));
        }

        private void ThenTheLoadBalancerIsCalled()
        {
            _loadBalancer.Verify(x => x.Release(It.IsAny<ServiceHostAndPort>()), Times.Once);
        }

        private void WhenTheMessagesAreProcessed()
        {
            _bus.Process();
        }

        private void GivenIHackAMessageInWithAPastExpiry()
        {
            var hostAndPort = new ServiceHostAndPort("999", 999);
            _bus.Publish(new StickySession(hostAndPort, DateTime.UtcNow.AddDays(-1), "321"), 0);
        }

        private void ThenAnErrorIsReturned()
        {
            _result.IsError.ShouldBeTrue();
        }

        private void GivenTheLoadBalancerReturnsError()
        {
            _loadBalancer
                .Setup(x => x.Lease(It.IsAny<HttpContext>()))
                .ReturnsAsync(new ErrorResponse<ServiceHostAndPort>(new AnyError()));
        }

        private void ThenADifferentHostAndPortIsReturned()
        {
            _firstHostAndPort.Data.DownstreamHost.ShouldBe("one");
            _firstHostAndPort.Data.DownstreamPort.ShouldBe(80);
            _secondHostAndPort.Data.DownstreamHost.ShouldBe("two");
            _secondHostAndPort.Data.DownstreamPort.ShouldBe(80);
        }

        private async Task WhenIMakeTwoRequetsWithDifferentSessionValues()
        {
            var contextOne = new DefaultHttpContext();
            var cookiesOne = new FakeCookies();
            cookiesOne.AddCookie("sessionid", "321");
            contextOne.Request.Cookies = cookiesOne;
            var contextTwo = new DefaultHttpContext();
            var cookiesTwo = new FakeCookies();
            cookiesTwo.AddCookie("sessionid", "123");
            contextTwo.Request.Cookies = cookiesTwo;
            _firstHostAndPort = await _stickySessions.Lease(contextOne);
            _secondHostAndPort = await _stickySessions.Lease(contextTwo);
        }

        private void GivenTheLoadBalancerReturnsSequence()
        {
            _loadBalancer
                .SetupSequence(x => x.Lease(It.IsAny<HttpContext>()))
                .ReturnsAsync(new OkResponse<ServiceHostAndPort>(new ServiceHostAndPort("one", 80)))
                .ReturnsAsync(new OkResponse<ServiceHostAndPort>(new ServiceHostAndPort("two", 80)));
        }

        private void ThenTheFirstAndSecondResponseAreTheSame()
        {
            _firstHostAndPort.Data.DownstreamHost.ShouldBe(_secondHostAndPort.Data.DownstreamHost);
            _firstHostAndPort.Data.DownstreamPort.ShouldBe(_secondHostAndPort.Data.DownstreamPort);
        }

        private async Task WhenILeaseTwiceInARow()
        {
            _firstHostAndPort = await _stickySessions.Lease(_httpContext);
            _secondHostAndPort = await _stickySessions.Lease(_httpContext);
        }

        private void GivenTheDownstreamRequestHasSessionId(string value)
        {
            var context = new DefaultHttpContext();
            var cookies = new FakeCookies();
            cookies.AddCookie("sessionid", value);
            context.Request.Cookies = cookies;
            _httpContext = context;
        }

        private void GivenTheLoadBalancerReturns()
        {
            _loadBalancer
                .Setup(x => x.Lease(It.IsAny<HttpContext>()))
                .ReturnsAsync(new OkResponse<ServiceHostAndPort>(new ServiceHostAndPort("", 80)));
        }

        private async Task WhenILease()
        {
            _result = await _stickySessions.Lease(_httpContext);
        }

        private void ThenTheHostAndPortIsNotNull()
        {
            _result.Data.ShouldNotBeNull();
        }

        private void ThenTheStickySessionWillTimeout()
        {
            _bus.Messages.Count.ShouldBe(2);
        }
    }

    internal class FakeCookies : IRequestCookieCollection
    {
        private readonly Dictionary<string, string> _cookies = new Dictionary<string, string>();

        public string this[string key] => _cookies[key];

        public int Count => _cookies.Count;

        public ICollection<string> Keys => _cookies.Keys;

        public void AddCookie(string key, string value)
        {
            _cookies[key] = value;
        }

        public bool ContainsKey(string key)
        {
            return _cookies.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _cookies.GetEnumerator();
        }

        public bool TryGetValue(string key, out string value)
        {
            return _cookies.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _cookies.GetEnumerator();
        }
    }

    internal class FakeBus<T> : IBus<T>
    {
        public FakeBus()
        {
            Messages = new List<T>();
            Subscriptions = new List<Action<T>>();
        }

        public List<T> Messages { get; }
        public List<Action<T>> Subscriptions { get; }

        public void Subscribe(Action<T> action)
        {
            Subscriptions.Add(action);
        }

        public void Publish(T message, int delay)
        {
            Messages.Add(message);
        }

        public void Process()
        {
            foreach (var message in Messages)
            {
                foreach (var subscription in Subscriptions)
                {
                    subscription(message);
                }
            }
        }
    }
}
