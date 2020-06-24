Websockets
==========

Ocelot supports proxying websockets with some extra bits. This functionality was requested in `Issue 212 <https://github.com/ThreeMammals/Ocelot/issues/212>`_. 

In order to get websocket proxying working with Ocelot you need to do the following.

In your Configure method you need to tell your application to use WebSockets.

.. code-block:: csharp

     Configure(app =>
    {
        app.UseWebSockets();
        app.UseOcelot().Wait();
    })

Then in your ocelot.json add the following to proxy a Route using websockets.

.. code-block:: json

       {
            "DownstreamPathTemplate": "/ws",
            "UpstreamPathTemplate": "/",
            "DownstreamScheme": "ws",
            "DownstreamHostAndPorts": [
                {
                    "Host": "localhost",
                    "Port": 5001
                }
            ],
        }

With this configuration set Ocelot will match any websocket traffic that comes in on / and proxy it to localhost:5001/ws. To make this clearer Ocelot will receive messages from the upstream client, proxy these to the downstream service, receive messages from the downstream service and proxy these to the upstream client.

SignalR
^^^^^^^

Ocelot supports proxying SignalR. This functionality was requested in `Issue 344 <https://github.com/ThreeMammals/Ocelot/issues/344>`_. 

In order to get websocket proxying working with Ocelot you need to do the following.

Install Microsoft.AspNetCore.SignalR.Client 1.0.2 you can try other packages but this one is tested.

Do not run it in IISExpress or install the websockets feature in the IIS features

In your Configure method you need to tell your application to use SignalR.

.. code-block:: csharp

     Configure(app =>
    {
        app.UseWebSockets();
        app.UseOcelot().Wait();
    })

Then in your ocelot.json add the following to proxy a Route using SignalR. Note normal Ocelot routing rules apply the main thing is the scheme which is set to "ws".

.. code-block:: json

   {
  "Routes": [
    {
      "DownstreamPathTemplate": "/{catchAll}",
      "DownstreamScheme": "ws",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 50000
        }
      ],
      "UpstreamPathTemplate": "/gateway/{catchAll}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE", "OPTIONS" ]
    }
 ]
}

With this configuration set Ocelot will match any SignalR traffic that comes in on / and proxy it to localhost:5001/ws. To make this clearer Ocelot will receive messages from the upstream client, proxy these to the downstream service, receive messages from the downstream service and proxy these to the upstream client.

Supported
^^^^^^^^^

1. Load Balancer
2. Routing
3. Service Discovery

This means that you can set up your downstream services running websockets and either have multiple DownstreamHostAndPorts in your Route config or hook your Route into a service discovery provider and then load balance requests...Which I think is pretty cool :)

Not Supported
^^^^^^^^^^^^^

Unfortunately a lot of Ocelot's features are non websocket specific such as header and http client stuff. I've listed what won't work below.

1. Tracing
2. RequestId
3. Request Aggregation
4. Rate Limiting
5. Quality of Service
6. Middleware Injection
7. Header Transformation
8. Delegating Handlers
9. Claims Transformation
10. Caching
11. Authentication - If anyone requests it we might be able to do something with basic authentication.
12. Authorisation

I'm not 100% sure what will happen with this feature when it get's into the wild so please make sure you test thoroughly! 


