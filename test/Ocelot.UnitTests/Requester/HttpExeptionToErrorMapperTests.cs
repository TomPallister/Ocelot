namespace Ocelot.UnitTests.Requester
{
    using Microsoft.Extensions.DependencyInjection;
    using Ocelot.Errors;
    using Ocelot.Requester;
    using Responder;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;

    public class HttpExeptionToErrorMapperTests
    {
        private HttpExeptionToErrorMapper _mapper;
        private readonly ServiceCollection _services;

        public HttpExeptionToErrorMapperTests()
        {
            _services = new ServiceCollection();
            var provider = _services.BuildServiceProvider();
            _mapper = new HttpExeptionToErrorMapper(provider);
        }

        [Fact]
        public void should_return_default_error_because_mappers_are_null()
        {
            var error = _mapper.Map(new Exception());

            error.ShouldBeOfType<UnableToCompleteRequestError>();
        }

        [Fact]
        public void should_return_request_canceled()
        {
            var error = _mapper.Map(new OperationCanceledException());

            error.ShouldBeOfType<RequestCanceledError>();
        }

        [Fact]
        public void should_return_ConnectionToDownstreamServiceError()
        {
            var error = _mapper.Map(new HttpRequestException());

            error.ShouldBeOfType<ConnectionToDownstreamServiceError>();
        }

        [Fact]
        public void should_return_request_canceled_for_subtype()
        {
            var error = _mapper.Map(new SomeException());

            error.ShouldBeOfType<RequestCanceledError>();
        }

        [Fact]
        public void should_return_error_from_mapper()
        {
            var errorMapping = new Dictionary<Type, Func<Exception, Error>>
            {
                {typeof(TaskCanceledException), e => new AnyError()},
            };

            _services.AddSingleton(errorMapping);

            var provider = _services.BuildServiceProvider();

            _mapper = new HttpExeptionToErrorMapper(provider);

            var error = _mapper.Map(new TaskCanceledException());

            error.ShouldBeOfType<AnyError>();
        }

        private class SomeException : OperationCanceledException
        { }
    }
}
