Routing
=======

Ocelot's primary functionality is to take incoming http requests and forward them on to a downstream service. Ocelot currently only supports this in the form of another http request (in the future
this could be any transport mechanism). 

Ocelot's describes the routing of one request to another as a Route. In order to get anything working in Ocelot you need to set up a Route in the configuration.

.. code-block:: json

    {
        "Routes": [
        ]
    }

To configure a Route you need to add one to the Routes json array.

.. code-block:: json

    {
        "DownstreamPathTemplate": "/api/posts/{postId}",
        "DownstreamScheme": "https",
        "DownstreamHostAndPorts": [
                {
                    "Host": "localhost",
                    "Port": 80,
                }
            ],
        "UpstreamPathTemplate": "/posts/{postId}",
        "UpstreamHttpMethod": [ "Put", "Delete" ]
    }

The DownstreamPathTemplate, DownstreamScheme and DownstreamHostAndPorts define the URL that a request will be forwarded to. 

DownstreamHostAndPorts is a collection that defines the host and port of any downstream services that you wish to forward requests to. Usually this will just contain a single entry but sometimes you might want to load balance requests to your downstream services and Ocelot allows you add more than one entry and then select a load balancer.

The UpstreamPathTemplate is the URL that Ocelot will use to identify which DownstreamPathTemplate to use for a given request. The UpstreamHttpMethod is used so Ocelot can distinguish between requests with different HTTP verbs to the same URL. You can set a specific list of HTTP Methods or set an empty list to allow any of them. 

In Ocelot you can add placeholders for variables to your Templates in the form of {something}. The placeholder variable needs to be present in both the DownstreamPathTemplate and UpstreamPathTemplate properties. When it is Ocelot will attempt to substitute the value in the UpstreamPathTemplate placeholder into the DownstreamPathTemplate for each request Ocelot processes.

You can also do a catch all type of Route e.g. 

.. code-block:: json

    {
        "DownstreamPathTemplate": "/api/{everything}",
        "DownstreamScheme": "https",
        "DownstreamHostAndPorts": [
                {
                    "Host": "localhost",
                    "Port": 80,
                }
            ],
        "UpstreamPathTemplate": "/{everything}",
        "UpstreamHttpMethod": [ "Get", "Post" ]
    }

This will forward any path + query string combinations to the downstream service after the path /api.


The default ReRouting configuration is case insensitive!

In order to change this you can specify on a per Route basis the following setting.

.. code-block:: json

    "RouteIsCaseSensitive": true

This means that when Ocelot tries to match the incoming upstream url with an upstream template the
evaluation will be case sensitive. 

Catch All
^^^^^^^^^

Ocelot's routing also supports a catch all style routing where the user can specify that they want to match all traffic.

If you set up your config like below, all requests will be proxied straight through. The placeholder {url} name is not significant, any name will work.

.. code-block:: json

    {
        "DownstreamPathTemplate": "/{url}",
        "DownstreamScheme": "https",
        "DownstreamHostAndPorts": [
                {
                    "Host": "localhost",
                    "Port": 80,
                }
            ],
        "UpstreamPathTemplate": "/{url}",
        "UpstreamHttpMethod": [ "Get" ]
    }

The catch all has a lower priority than any other Route. If you also have the Route below in your config then Ocelot would match it before the catch all. 

.. code-block:: json

    {
        "DownstreamPathTemplate": "/",
        "DownstreamScheme": "https",
        "DownstreamHostAndPorts": [
                {
                    "Host": "10.0.10.1",
                    "Port": 80,
                }
            ],
        "UpstreamPathTemplate": "/",
        "UpstreamHttpMethod": [ "Get" ]
    }

Upstream Host 
^^^^^^^^^^^^^

This feature allows you to have Routes based on the upstream host. This works by looking at the host header the client has used and then using this as part of the information we use to identify a Route.

In order to use this feature please add the following to your config.

.. code-block:: json

    {
        "DownstreamPathTemplate": "/",
        "DownstreamScheme": "https",
        "DownstreamHostAndPorts": [
                {
                    "Host": "10.0.10.1",
                    "Port": 80,
                }
            ],
        "UpstreamPathTemplate": "/",
        "UpstreamHttpMethod": [ "Get" ],
        "UpstreamHost": "somedomain.com"
    }

The Route above will only be matched when the host header value is somedomain.com.

If you do not set UpstreamHost on a Route then any host header will match it. This means that if you have two Routes that are the same, apart from the UpstreamHost, where one is null and the other set Ocelot will favour the one that has been set. 

This feature was requested as part of `Issue 216 <https://github.com/ThreeMammals/Ocelot/pull/216>`_ .

Priority
^^^^^^^^

You can define the order you want your Routes to match the Upstream HttpRequest by including a "Priority" property in ocelot.json
See `Issue 270 <https://github.com/ThreeMammals/Ocelot/pull/270>`_ for reference

.. code-block:: json

    {
        "Priority": 0
    }

0 is the lowest priority, Ocelot will always use 0 for /{catchAll} Routes and this is still hardcoded. After that you are free to set any priority you wish.

e.g. you could have

.. code-block:: json

    {
        "UpstreamPathTemplate": "/goods/{catchAll}"
        "Priority": 0
    }

and 

.. code-block:: json

    {
        "UpstreamPathTemplate": "/goods/delete"
        "Priority": 1
    }

In the example above if you make a request into Ocelot on /goods/delete Ocelot will match /goods/delete Route. Previously it would have matched /goods/{catchAll} (because this is the first Route in the list!).

Dynamic Routing
^^^^^^^^^^^^^^^

This feature was requested in `issue 340 <https://github.com/ThreeMammals/Ocelot/issues/340>`_. 

The idea is to enable dynamic routing when using a service discovery provider so you don't have to provide the Route config. See the docs :ref:`service-discovery` if 
this sounds interesting to you.

Query Strings
^^^^^^^^^^^^^

Ocelot allows you to specify a query string as part of the DownstreamPathTemplate like the example below.

.. code-block:: json

    {
        "Routes": [
            {
                "DownstreamPathTemplate": "/api/subscriptions/{subscriptionId}/updates?unitId={unitId}",
                "UpstreamPathTemplate": "/api/units/{subscriptionId}/{unitId}/updates",
                "UpstreamHttpMethod": [
                    "Get"
                ],
                "DownstreamScheme": "http",
                "DownstreamHostAndPorts": [
                    {
                        "Host": "localhost",
                        "Port": 50110
                    }
                ]
            }
        ],
        "GlobalConfiguration": {
        }
    }

In this example Ocelot will use the value from the {unitId} in the upstream path template and add it to the downstream request as a query string parameter called unitId!

Ocelot will also allow you to put query string parameters in the UpstreamPathTemplate so you can match certain queries to certain services.

.. code-block:: json

    {
        "Routes": [
            {
                "DownstreamPathTemplate": "/api/units/{subscriptionId}/{unitId}/updates",
                "UpstreamPathTemplate": "/api/subscriptions/{subscriptionId}/updates?unitId={unitId}",
                "UpstreamHttpMethod": [
                    "Get"
                ],
                "DownstreamScheme": "http",
                "DownstreamHostAndPorts": [
                    {
                        "Host": "localhost",
                        "Port": 50110
                    }
                ]
            }
        ],
        "GlobalConfiguration": {
        }
    }

In this example Ocelot will only match requests that have a matching url path and the query string starts with unitId=something. You can have other queries after this
but you must start with the matching parameter. Also Ocelot will swap the {unitId} parameter from the query string and use it in the downstream request path. 
