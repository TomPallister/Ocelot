using Ocelot.Configuration;
using Ocelot.Responses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Ocelot.LoadBalancer.LoadBalancers
{
    public class LoadBalancerHouse : ILoadBalancerHouse
    {
        private readonly ILoadBalancerFactory _factory;
        private readonly ConcurrentDictionary<string, ILoadBalancer> _loadBalancers;

        public LoadBalancerHouse(ILoadBalancerFactory factory)
        {
            _factory = factory;
            _loadBalancers = new ConcurrentDictionary<string, ILoadBalancer>();
        }

        public Response<ILoadBalancer> Get(DownstreamRoute route, ServiceProviderConfiguration config)
        {
            try
            {
                Response<ILoadBalancer> result;

                if (_loadBalancers.TryGetValue(route.LoadBalancerKey, out var loadBalancer))
                {
                    loadBalancer = _loadBalancers[route.LoadBalancerKey];

                    if (route.LoadBalancerOptions.Type != loadBalancer.GetType().Name)
                    {
                        result = _factory.Get(route, config);
                        if (result.IsError)
                        {
                            return new ErrorResponse<ILoadBalancer>(result.Errors);
                        }

                        loadBalancer = result.Data;
                        AddLoadBalancer(route.LoadBalancerKey, loadBalancer);
                    }

                    return new OkResponse<ILoadBalancer>(loadBalancer);
                }

                result = _factory.Get(route, config);

                if (result.IsError)
                {
                    return new ErrorResponse<ILoadBalancer>(result.Errors);
                }

                loadBalancer = result.Data;
                AddLoadBalancer(route.LoadBalancerKey, loadBalancer);
                return new OkResponse<ILoadBalancer>(loadBalancer);
            }
            catch (Exception ex)
            {
                return new ErrorResponse<ILoadBalancer>(new List<Ocelot.Errors.Error>()
                {
                    new UnableToFindLoadBalancerError($"unabe to find load balancer for {route.LoadBalancerKey} exception is {ex}"),
                });
            }
        }

        private void AddLoadBalancer(string key, ILoadBalancer loadBalancer)
        {
            _loadBalancers.AddOrUpdate(key, loadBalancer, (x, y) => loadBalancer);
        }
    }
}
