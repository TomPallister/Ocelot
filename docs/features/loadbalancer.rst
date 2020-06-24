Load Balancer
=============

Ocelot can load balance across available downstream services for each Route. This means you can scale your downstream services and Ocelot can use them effectively.

The type of load balancer available are:
    
    LeastConnection - tracks which services are dealing with requests and sends new requests to service with least existing requests. The algorythm state is not distributed across a cluster of Ocelot's.

    RoundRobin - loops through available services and sends requests. The algorythm state is not distributed across a cluster of Ocelot's.
    
    NoLoadBalancer - takes the first available service from config or service discovery.

    CookieStickySessions - uses a cookie to stick all requests to a specific server. More info below.

You must choose in your configuration which load balancer to use.

Configuration
^^^^^^^^^^^^^

The following shows how to set up multiple downstream services for a Route using ocelot.json and then select the LeastConnection load balancer. This is the simplest way to get load balancing set up.

.. code-block:: json

    {
        "DownstreamPathTemplate": "/api/posts/{postId}",
        "DownstreamScheme": "https",
        "DownstreamHostAndPorts": [
                {
                    "Host": "10.0.1.10",
                    "Port": 5000,
                },
                {
                    "Host": "10.0.1.11",
                    "Port": 5000,
                }
            ],
        "UpstreamPathTemplate": "/posts/{postId}",
        "LoadBalancerOptions": {
            "Type": "LeastConnection"
        },
        "UpstreamHttpMethod": [ "Put", "Delete" ]
    }


Service Discovery
^^^^^^^^^^^^^^^^^

The following shows how to set up a Route using service discovery then select the LeastConnection load balancer.

.. code-block:: json

    {
        "DownstreamPathTemplate": "/api/posts/{postId}",
        "DownstreamScheme": "https",
        "UpstreamPathTemplate": "/posts/{postId}",
        "UpstreamHttpMethod": [ "Put" ],
        "ServiceName": "product",
        "LoadBalancerOptions": {
            "Type": "LeastConnection"
        },
    }

When this is set up Ocelot will lookup the downstream host and port from the service discover provider and load balance requests across any available services. If you add and remove services from the 
service discovery provider (consul) then Ocelot should respect this and stop calling services that have been removed and start calling services that have been added.

CookieStickySessions
^^^^^^^^^^^^^^^^^^^^

I've implemented a really basic sticky session type of load balancer. The scenario it is meant to support is you have a bunch of downstream servers that don't share session state so if you get more than one request for one of these servers then it should go to the same box each time or the session state might be incorrect for the given user. This feature was requested in `Issue #322 <https://github.com/ThreeMammals/Ocelot/issues/322>`_ though what the user wants is more complicated than just sticky sessions :) anyway I thought this would be a nice feature to have!

In order to set up CookieStickySessions load balancer you need to do something like the following.

.. code-block:: json

    {
        "DownstreamPathTemplate": "/api/posts/{postId}",
        "DownstreamScheme": "https",
        "DownstreamHostAndPorts": [
                {
                    "Host": "10.0.1.10",
                    "Port": 5000,
                },
                {
                    "Host": "10.0.1.11",
                    "Port": 5000,
                }
            ],
        "UpstreamPathTemplate": "/posts/{postId}",
        "LoadBalancerOptions": {
            "Type": "CookieStickySessions",
            "Key": "ASP.NET_SessionId",
            "Expiry": 1800000
        },
        "UpstreamHttpMethod": [ "Put", "Delete" ]
    }

The LoadBalancerOptions are Type this needs to be CookieStickySessions, Key this is the key of the cookie you wish to use for the sticky sessions, Expiry this is how long in milliseconds you want to the session to be stuck for. Remember this refreshes on every request which is meant to mimick how sessions work usually.

If you have multiple Routes with the same LoadBalancerOptions then all of those Routes will use the same load balancer for there subsequent requests. This means the sessions will be stuck across Routes.

Please note that if you give more than one DownstreamHostAndPort or you are using a Service Discovery provider such as Consul and this returns more than one service then CookieStickySessions uses round robin to select the next server. This is hard coded at the moment but could be changed.

Custom Load Balancers
^^^^^^^^^^^^^^^^^^^^

`DavidLievrouw <https://github.com/DavidLievrouw`_ implemented a way to provide Ocelot with custom load balancer in `PR 1155 <https://github.com/ThreeMammals/Ocelot/pull/1155`_.

In order to create and use a custom load balancer you can do the following. Below we setup a basic load balancing config and not the Type is CustomLoadBalancer this is the name of a class we will setup to do load balancing.

.. code-block:: json

    {
        "DownstreamPathTemplate": "/api/posts/{postId}",
        "DownstreamScheme": "https",
        "DownstreamHostAndPorts": [
                {
                    "Host": "10.0.1.10",
                    "Port": 5000,
                },
                {
                    "Host": "10.0.1.11",
                    "Port": 5000,
                }
            ],
        "UpstreamPathTemplate": "/posts/{postId}",
        "LoadBalancerOptions": {
            "Type": "CustomLoadBalancer"
        },
        "UpstreamHttpMethod": [ "Put", "Delete" ]
    }


Then you need to create a class that implements the ILoadBalancer interface. Below is a simple round robin example.

.. code-block:: csharp

        private class CustomLoadBalancer : ILoadBalancer
        {
            private readonly Func<Task<List<Service>>> _services;
            private readonly object _lock = new object();

            private int _last;

            public CustomLoadBalancer(Func<Task<List<Service>>> services)
            {
                _services = services;
            }

            public async Task<Response<ServiceHostAndPort>> Lease(DownstreamContext downstreamContext, HttpContext httpContext)
            {
                var services = await _services();
                lock (_lock)
                {
                    if (_last >= services.Count)
                    {
                        _last = 0;
                    }

                    var next = services[_last];
                    _last++;
                    return new OkResponse<ServiceHostAndPort>(next.HostAndPort);
                }
            }

            public void Release(ServiceHostAndPort hostAndPort)
            {
            }
        }

Finally you need to register this class with Ocelot. I have used the most complex example below to show all of the data / types that can be passed into the factory that creates load balancers.

.. code-block:: csharp

            Func<IServiceProvider, DownstreamRoute, IServiceDiscoveryProvider, CustomLoadBalancer> loadBalancerFactoryFunc = (serviceProvider, Route, serviceDiscoveryProvider) => new CustomLoadBalancer(serviceDiscoveryProvider.Get);

            s.AddOcelot()
                .AddCustomLoadBalancer(loadBalancerFactoryFunc);

However there is a much simpler example that will work the same.

.. code-block:: csharp

            s.AddOcelot()
                .AddCustomLoadBalancer<CustomLoadBalancer>();

There are numerous extension methods to add a custom load balancer and the interface is as follows.

.. code-block:: csharp

        IOcelotBuilder AddCustomLoadBalancer<T>()
            where T : ILoadBalancer, new();

         IOcelotBuilder AddCustomLoadBalancer<T>(Func<T> loadBalancerFactoryFunc)
            where T : ILoadBalancer;

         IOcelotBuilder AddCustomLoadBalancer<T>(Func<IServiceProvider, T> loadBalancerFactoryFunc)
            where T : ILoadBalancer;

         IOcelotBuilder AddCustomLoadBalancer<T>(
            Func<DownstreamRoute, IServiceDiscoveryProvider, T> loadBalancerFactoryFunc)
            where T : ILoadBalancer;

         IOcelotBuilder AddCustomLoadBalancer<T>(
            Func<IServiceProvider, DownstreamRoute, IServiceDiscoveryProvider, T> loadBalancerFactoryFunc)
            where T : ILoadBalancer;

When you enable custom load balancers Ocelot looks up your load balancer by its class name when it decides if it should do load balancing. If it finds a match it will use your load balaner to load balance. If Ocelot cannot match the load balancer type in your configuration with the name of registered load balancer class then you will receive a HTTP 500 internal server error. If your load balancer factory throw an exception when Ocelot calls it you will receive a HTTP 500 internal server error.

Remember if you specify no load balancer in your config Ocelot will not try and load balance.