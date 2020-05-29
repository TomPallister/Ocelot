using Microsoft.Extensions.DependencyInjection;
using Ocelot.Configuration;
using Ocelot.Responses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ocelot.Multiplexer
{
    public class ServiceLocatorDefinedAggregatorProvider : IDefinedAggregatorProvider
    {
        private readonly Dictionary<string, IDefinedAggregator> _aggregators;

        public ServiceLocatorDefinedAggregatorProvider(IServiceProvider services)
        {
            _aggregators = services.GetServices<IDefinedAggregator>().ToDictionary(x => x.GetType().Name);
        }

        public Response<IDefinedAggregator> Get(Route route)
        {
            if (_aggregators.ContainsKey(route.Aggregator))
            {
                return new OkResponse<IDefinedAggregator>(_aggregators[route.Aggregator]);
            }

            return new ErrorResponse<IDefinedAggregator>(new CouldNotFindAggregatorError(route.Aggregator));
        }
    }
}
