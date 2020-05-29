.. service-discovery:

Service Discovery
=================

Ocelot allows you to specify a service discovery provider and will use this to find the host and port for the downstream service Ocelot is forwarding a request to. At the moment this is only supported in the
GlobalConfiguration section which means the same service discovery provider will be used for all Routes you specify a ServiceName for at Route level. 

Consul
^^^^^^

The first thing you need to do is install the NuGet package that provides Consul support in Ocelot.

``Install-Package Ocelot.Provider.Consul``

Then add the following to your ConfigureServices method.

.. code-block:: csharp

    s.AddOcelot()
        .AddConsul();

The following is required in the GlobalConfiguration. The Provider is required and if you do not specify a host and port the Consul default
will be used.

Please note the Scheme option defauls to HTTP. It was added in this `PR <https://github.com/ThreeMammals/Ocelot/pull/1154>`_. It defaults to HTTP to not introduce a breaking change.

.. code-block:: json

    "ServiceDiscoveryProvider": {
        "Scheme": "https",
        "Host": "localhost",
        "Port": 8500,
        "Type": "Consul"
    }

In the future we can add a feature that allows Route specfic configuration. 

In order to tell Ocelot a Route is to use the service discovery provider for its host and port you must add the ServiceName and load balancer you wish to use when making requests downstream. At the moment Ocelot has a RoundRobin and LeastConnection algorithm you can use. If no load balancer is specified Ocelot will not load balance requests.

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

When this is set up Ocelot will lookup the downstream host and port from the service discover provider and load balance requests across any available services.

A lot of people have asked me to implement a feature where Ocelot polls Consul for latest service information rather than per request. If you want to poll Consul for the latest services rather than per request (default behaviour) then you need to set the following configuration.

.. code-block:: json

    "ServiceDiscoveryProvider": {
        "Host": "localhost",
        "Port": 8500,
        "Type": "PollConsul",
        "PollingInterval": 100
    }

The polling interval is in milliseconds and tells Ocelot how often to call Consul for changes in service configuration.

Please note there are tradeoffs here. If you poll Consul it is possible Ocelot will not know if a service is down depending on your polling interval and you might get more errors than if you get the latest services per request. This really depends on how volatile your services are. I doubt it will matter for most people and polling may give a tiny performance improvement over calling Consul per request (as sidecar agent). If you are calling a remote Consul agent then polling will be a good performance improvement.

Your services need to be added to Consul something like below (C# style but hopefully this make sense)...The only important thing to note is not to add http or https to the Address field. I have been contacted before about not accepting scheme in Address and accepting scheme in address. After reading `this <https://www.consul.io/docs/agent/services.html>`_ I don't think the scheme should be in there.

.. code-block: csharp

    new AgentService()
    {
        Service = "some-service-name",
        Address = "localhost",
        Port = 8080,
        ID = "some-id",
    }

Or 

.. code-block:: json

     "Service": {
            "ID": "some-id",
            "Service": "some-service-name",
            "Address": "localhost",
            "Port": 8080
        }

ACL Token
---------

If you are using ACL with Consul Ocelot supports adding the X-Consul-Token header. In order so this to work you must add the additional property below.

.. code-block:: json

    "ServiceDiscoveryProvider": {
        "Host": "localhost",
        "Port": 8500,
        "Token": "footoken",
        "Type": "Consul"
    }

Ocelot will add this token to the Consul client that it uses to make requests and that is then used for every request.

Eureka
^^^^^^

This feature was requested as part of `Issue 262 <https://github.com/ThreeMammals/Ocelot/issues/262>`_ . to add support for Netflix's Eureka service discovery provider. The main reason for this is it is a key part of  `Steeltoe <https://steeltoe.io/>`_ which is something to do with `Pivotal <https://pivotal.io/platform>`_! Anyway enough of the background.

The first thing you need to do is install the NuGet package that provides Eureka support in Ocelot.

``Install-Package Ocelot.Provider.Eureka``

Then add the following to your ConfigureServices method.

.. code-block:: csharp

    s.AddOcelot()
        .AddEureka();

Then in order to get this working add the following to ocelot.json..

.. code-block:: json

    "ServiceDiscoveryProvider": {
        "Type": "Eureka"
    }

And following the guide `Here <https://steeltoe.io/docs/steeltoe-discovery/>`_ you may also need to add some stuff to appsettings.json. For example the json below tells the steeltoe / pivotal services where to look for the service discovery server and if the service should register with it.

.. code-block:: json

    "eureka": {
        "client": {
        "serviceUrl": "http://localhost:8761/eureka/",
        "shouldRegisterWithEureka": false,
        "shouldFetchRegistry": true
        }
    }

I am told that if shouldRegisterWithEureka is false then shouldFetchRegistry will defaut to true so you don't need it explicitly but left it in there.

Ocelot will now register all the necessary services when it starts up and if you have the json above will register itself with Eureka. One of the services polls Eureka every 30 seconds (default) and gets the latest service state and persists this in memory. When Ocelot asks for a given service it is retrieved from memory so performance is not a big problem. Please note that this code is provided by the Pivotal.Discovery.Client NuGet package so big thanks to them for all the hard work.

Ocelot will use the scheme (http/https) set in Eureka if these values are not provided in ocelot.json

Dynamic Routing
^^^^^^^^^^^^^^^

This feature was requested in `issue 340 <https://github.com/ThreeMammals/Ocelot/issues/340>`_. The idea is to enable dynamic routing when using a service discovery provider (see that section of the docs for more info). In this mode Ocelot will use the first segment of the upstream path to lookup the downstream service with the service discovery provider. 

An example of this would be calling Ocelot with a url like https://api.mywebsite.com/product/products. Ocelot will take the first segment of 
the path which is product and use it as a key to look up the service in Consul. If Consul returns a service Ocelot will request it on whatever host and port comes back from Consul plus the remaining path segments in this case products thus making the downstream call http://hostfromconsul:portfromconsul/products. Ocelot will apprend any query string to the downstream url as normal.

In order to enable dynamic routing you need to have 0 Routes in your config. At the moment you cannot mix dynamic and configuration Routes. In addition to this you need to specify the Service Discovery provider details as outlined above and the downstream http/https scheme as DownstreamScheme.

In addition to that you can set RateLimitOptions, QoSOptions, LoadBalancerOptions and HttpHandlerOptions, DownstreamScheme (You might want to call Ocelot on https but talk to private services over http) that will be applied to all of the dynamic Routes.

The config might look something like 

.. code-block:: json

    {
        "Routes": [],
        "Aggregates": [],
        "GlobalConfiguration": {
            "RequestIdKey": null,
            "ServiceDiscoveryProvider": {
                "Host": "localhost",
                "Port": 8500,
                "Type": "Consul",
                "Token": null,
                "ConfigurationKey": null
            },
            "RateLimitOptions": {
                "ClientIdHeader": "ClientId",
                "QuotaExceededMessage": null,
                "RateLimitCounterPrefix": "ocelot",
                "DisableRateLimitHeaders": false,
                "HttpStatusCode": 429
            },
            "QoSOptions": {
                "ExceptionsAllowedBeforeBreaking": 0,
                "DurationOfBreak": 0,
                "TimeoutValue": 0
            },
            "BaseUrl": null,
                "LoadBalancerOptions": {
                "Type": "LeastConnection",
                "Key": null,
                "Expiry": 0
            },
            "DownstreamScheme": "http",
            "HttpHandlerOptions": {
                "AllowAutoRedirect": false,
                "UseCookieContainer": false,
                "UseTracing": false
            }
        }
    }

Ocelot also allows you to set DynamicRoutes which lets you set rate limiting rules per downstream service. This is useful if you have for example a product and search service and you want to rate limit one more than the other. An example of this would be as follows.

.. code-block:: json

    {
        "DynamicRoutes": [
            {
            "ServiceName": "product",
            "RateLimitRule": {
                    "ClientWhitelist": [],
                    "EnableRateLimiting": true,
                    "Period": "1s",
                    "PeriodTimespan": 1000.0,
                    "Limit": 3
                }
            }
        ],
        "GlobalConfiguration": {
            "RequestIdKey": null,
            "ServiceDiscoveryProvider": {
                "Host": "localhost",
                "Port": 8523,
                "Type": "Consul"
            },
            "RateLimitOptions": {
                "ClientIdHeader": "ClientId",
                "QuotaExceededMessage": "",
                "RateLimitCounterPrefix": "",
                "DisableRateLimitHeaders": false,
                "HttpStatusCode": 428
            }
            "DownstreamScheme": "http",
        }
    }

This configuration means that if you have a request come into Ocelot on /product/* then dynamic routing will kick in and ocelot will use the rate limiting set against the product service in the DynamicRoutes section.

Please take a look through all of the docs to understand these options.
