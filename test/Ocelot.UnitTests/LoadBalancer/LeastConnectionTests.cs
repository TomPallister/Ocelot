using Microsoft.AspNetCore.Http;
using Ocelot.LoadBalancer.LoadBalancers;
using Ocelot.Middleware;
using Ocelot.Responses;
using Ocelot.Values;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.LoadBalancer
{
    public class LeastConnectionTests
    {
        private ServiceHostAndPort _hostAndPort;
        private Response<ServiceHostAndPort> _result;
        private LeastConnection _leastConnection;
        private List<Service> _services;
        private Random _random;
        private HttpContext _httpContext;

        public LeastConnectionTests()
        {
            _httpContext = new DefaultHttpContext();
            _random = new Random();
        }

        [Fact]
        public void should_be_able_to_lease_and_release_concurrently()
        {
            var serviceName = "products";

            var availableServices = new List<Service>
            {
                new Service(serviceName, new ServiceHostAndPort("127.0.0.1", 80), string.Empty, string.Empty, new string[0]),
                new Service(serviceName, new ServiceHostAndPort("127.0.0.2", 80), string.Empty, string.Empty, new string[0]),
            };

            _services = availableServices;
            _leastConnection = new LeastConnection(() => Task.FromResult(_services), serviceName);

            var tasks = new Task[100];

            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i] = LeaseDelayAndRelease();
            }

            Task.WaitAll(tasks);
        }

        [Fact]
        public void should_handle_service_returning_to_available()
        {
            var serviceName = "products";

            var availableServices = new List<Service>
            {
                new Service(serviceName, new ServiceHostAndPort("127.0.0.1", 80), string.Empty, string.Empty, new string[0]),
                new Service(serviceName, new ServiceHostAndPort("127.0.0.2", 80), string.Empty, string.Empty, new string[0]),
            };

            _leastConnection = new LeastConnection(() => Task.FromResult(availableServices), serviceName);

            var hostAndPortOne = _leastConnection.Lease(_httpContext).Result;
            hostAndPortOne.Data.DownstreamHost.ShouldBe("127.0.0.1");
            var hostAndPortTwo = _leastConnection.Lease(_httpContext).Result;
            hostAndPortTwo.Data.DownstreamHost.ShouldBe("127.0.0.2");
            _leastConnection.Release(hostAndPortOne.Data);
            _leastConnection.Release(hostAndPortTwo.Data);

            availableServices = new List<Service>
            {
                new Service(serviceName, new ServiceHostAndPort("127.0.0.1", 80), string.Empty, string.Empty, new string[0]),
            };

            hostAndPortOne = _leastConnection.Lease(_httpContext).Result;
            hostAndPortOne.Data.DownstreamHost.ShouldBe("127.0.0.1");
            hostAndPortTwo = _leastConnection.Lease(_httpContext).Result;
            hostAndPortTwo.Data.DownstreamHost.ShouldBe("127.0.0.1");
            _leastConnection.Release(hostAndPortOne.Data);
            _leastConnection.Release(hostAndPortTwo.Data);

            availableServices = new List<Service>
            {
                new Service(serviceName, new ServiceHostAndPort("127.0.0.1", 80), string.Empty, string.Empty, new string[0]),
                new Service(serviceName, new ServiceHostAndPort("127.0.0.2", 80), string.Empty, string.Empty, new string[0]),
            };

            hostAndPortOne = _leastConnection.Lease(_httpContext).Result;
            hostAndPortOne.Data.DownstreamHost.ShouldBe("127.0.0.1");
            hostAndPortTwo = _leastConnection.Lease( _httpContext).Result;
            hostAndPortTwo.Data.DownstreamHost.ShouldBe("127.0.0.2");
            _leastConnection.Release(hostAndPortOne.Data);
            _leastConnection.Release(hostAndPortTwo.Data);
        }

        private async Task LeaseDelayAndRelease()
        {
            var hostAndPort = await _leastConnection.Lease(_httpContext);
            await Task.Delay(_random.Next(1, 100));
            _leastConnection.Release(hostAndPort.Data);
        }

        [Fact]
        public void should_get_next_url()
        {
            var serviceName = "products";

            var hostAndPort = new ServiceHostAndPort("localhost", 80);

            var availableServices = new List<Service>
            {
                new Service(serviceName, hostAndPort, string.Empty, string.Empty, new string[0])
            };

            this.Given(x => x.GivenAHostAndPort(hostAndPort))
            .And(x => x.GivenTheLoadBalancerStarts(availableServices, serviceName))
            .When(x => x.WhenIGetTheNextHostAndPort())
            .Then(x => x.ThenTheNextHostAndPortIsReturned())
            .BDDfy();
        }

        [Fact]
        public void should_serve_from_service_with_least_connections()
        {
            var serviceName = "products";

            var availableServices = new List<Service>
            {
                new Service(serviceName, new ServiceHostAndPort("127.0.0.1", 80), string.Empty, string.Empty, new string[0]),
                new Service(serviceName, new ServiceHostAndPort("127.0.0.2", 80), string.Empty, string.Empty, new string[0]),
                new Service(serviceName, new ServiceHostAndPort("127.0.0.3", 80), string.Empty, string.Empty, new string[0])
            };

            _services = availableServices;
            _leastConnection = new LeastConnection(() => Task.FromResult(_services), serviceName);

            var response = _leastConnection.Lease(_httpContext).Result;

            response.Data.DownstreamHost.ShouldBe(availableServices[0].HostAndPort.DownstreamHost);

            response = _leastConnection.Lease(_httpContext).Result;

            response.Data.DownstreamHost.ShouldBe(availableServices[1].HostAndPort.DownstreamHost);

            response = _leastConnection.Lease(_httpContext).Result;

            response.Data.DownstreamHost.ShouldBe(availableServices[2].HostAndPort.DownstreamHost);
        }

        [Fact]
        public void should_build_connections_per_service()
        {
            var serviceName = "products";

            var availableServices = new List<Service>
            {
                new Service(serviceName, new ServiceHostAndPort("127.0.0.1", 80), string.Empty, string.Empty, new string[0]),
                new Service(serviceName, new ServiceHostAndPort("127.0.0.2", 80), string.Empty, string.Empty, new string[0]),
            };

            _services = availableServices;
            _leastConnection = new LeastConnection(() => Task.FromResult(_services), serviceName);

            var response = _leastConnection.Lease(_httpContext).Result;

            response.Data.DownstreamHost.ShouldBe(availableServices[0].HostAndPort.DownstreamHost);

            response = _leastConnection.Lease(_httpContext).Result;

            response.Data.DownstreamHost.ShouldBe(availableServices[1].HostAndPort.DownstreamHost);

            response = _leastConnection.Lease(_httpContext).Result;

            response.Data.DownstreamHost.ShouldBe(availableServices[0].HostAndPort.DownstreamHost);

            response = _leastConnection.Lease(_httpContext).Result;

            response.Data.DownstreamHost.ShouldBe(availableServices[1].HostAndPort.DownstreamHost);
        }

        [Fact]
        public void should_release_connection()
        {
            var serviceName = "products";

            var availableServices = new List<Service>
            {
                new Service(serviceName, new ServiceHostAndPort("127.0.0.1", 80), string.Empty, string.Empty, new string[0]),
                new Service(serviceName, new ServiceHostAndPort("127.0.0.2", 80), string.Empty, string.Empty, new string[0]),
            };

            _services = availableServices;
            _leastConnection = new LeastConnection(() => Task.FromResult(_services), serviceName);

            var response = _leastConnection.Lease(_httpContext).Result;

            response.Data.DownstreamHost.ShouldBe(availableServices[0].HostAndPort.DownstreamHost);

            response = _leastConnection.Lease(_httpContext).Result;

            response.Data.DownstreamHost.ShouldBe(availableServices[1].HostAndPort.DownstreamHost);

            response = _leastConnection.Lease(_httpContext).Result;

            response.Data.DownstreamHost.ShouldBe(availableServices[0].HostAndPort.DownstreamHost);

            response = _leastConnection.Lease(_httpContext).Result;

            response.Data.DownstreamHost.ShouldBe(availableServices[1].HostAndPort.DownstreamHost);

            //release this so 2 should have 1 connection and we should get 2 back as our next host and port
            _leastConnection.Release(availableServices[1].HostAndPort);

            response = _leastConnection.Lease(_httpContext).Result;

            response.Data.DownstreamHost.ShouldBe(availableServices[1].HostAndPort.DownstreamHost);
        }

        [Fact]
        public void should_return_error_if_services_are_null()
        {
            var serviceName = "products";

            var hostAndPort = new ServiceHostAndPort("localhost", 80);
            this.Given(x => x.GivenAHostAndPort(hostAndPort))
             .And(x => x.GivenTheLoadBalancerStarts(null, serviceName))
             .When(x => x.WhenIGetTheNextHostAndPort())
             .Then(x => x.ThenServiceAreNullErrorIsReturned())
             .BDDfy();
        }

        [Fact]
        public void should_return_error_if_services_are_empty()
        {
            var serviceName = "products";

            var hostAndPort = new ServiceHostAndPort("localhost", 80);
            this.Given(x => x.GivenAHostAndPort(hostAndPort))
             .And(x => x.GivenTheLoadBalancerStarts(new List<Service>(), serviceName))
             .When(x => x.WhenIGetTheNextHostAndPort())
             .Then(x => x.ThenServiceAreEmptyErrorIsReturned())
             .BDDfy();
        }

        private void ThenServiceAreNullErrorIsReturned()
        {
            _result.IsError.ShouldBeTrue();
            _result.Errors[0].ShouldBeOfType<ServicesAreNullError>();
        }

        private void ThenServiceAreEmptyErrorIsReturned()
        {
            _result.IsError.ShouldBeTrue();
            _result.Errors[0].ShouldBeOfType<ServicesAreEmptyError>();
        }

        private void GivenTheLoadBalancerStarts(List<Service> services, string serviceName)
        {
            _services = services;
            _leastConnection = new LeastConnection(() => Task.FromResult(_services), serviceName);
        }

        private void WhenTheLoadBalancerStarts(List<Service> services, string serviceName)
        {
            GivenTheLoadBalancerStarts(services, serviceName);
        }

        private void GivenAHostAndPort(ServiceHostAndPort hostAndPort)
        {
            _hostAndPort = hostAndPort;
        }

        private void WhenIGetTheNextHostAndPort()
        {
            _result = _leastConnection.Lease(_httpContext).Result;
        }

        private void ThenTheNextHostAndPortIsReturned()
        {
            _result.Data.DownstreamHost.ShouldBe(_hostAndPort.DownstreamHost);
            _result.Data.DownstreamPort.ShouldBe(_hostAndPort.DownstreamPort);
        }
    }
}
