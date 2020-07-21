using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ocelot.UnitTests.Requester
{
    public class FakeDelegatingHandler : DelegatingHandler
    {
        public FakeDelegatingHandler()
        {
            Order = 1;
        }

        public FakeDelegatingHandler(int order)
        {
            Order = order;
        }

        public int Order { get; private set; }

        public DateTime TimeCalled { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            TimeCalled = DateTime.Now;
            return Task.FromResult(new HttpResponseMessage());
        }
    }

    public class FakeDelegatingHandlerThree : DelegatingHandler
    {
        public FakeDelegatingHandlerThree()
        {
            Order = 3;
        }

        public int Order { get; private set; }

        public DateTime TimeCalled { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            TimeCalled = DateTime.Now;
            return Task.FromResult(new HttpResponseMessage());
        }
    }

    public class FakeDelegatingHandlerFour : DelegatingHandler
    {
        public FakeDelegatingHandlerFour()
        {
            Order = 4;
        }

        public int Order { get; private set; }

        public DateTime TimeCalled { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            TimeCalled = DateTime.Now;
            return Task.FromResult(new HttpResponseMessage());
        }
    }

    public class FakeDelegatingHandlerTwo : DelegatingHandler
    {
        public FakeDelegatingHandlerTwo()
        {
            Order = 2;
        }

        public int Order { get; private set; }

        public DateTime TimeCalled { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            TimeCalled = DateTime.Now;
            return Task.FromResult(new HttpResponseMessage());
        }
    }
}
