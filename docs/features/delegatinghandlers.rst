Delegating Handlers
===================

Ocelot allows the user to add delegating handlers to the HttpClient transport. This feature was requested `GitHub #208 <https://github.com/ThreeMammals/Ocelot/issues/208>`_ 
and I decided that it was going to be useful in various ways. Since then we extended it in `GitHub #264 <https://github.com/ThreeMammals/Ocelot/issues/264>`_.

Usage
^^^^^

In order to add delegating handlers to the HttpClient transport you need to do two main things.

First in order to create a class that can be used a delegating handler it must look as follows. We are going to register these handlers in the 
asp.net core container so you can inject any other services you have registered into the constructor of your handler.

.. code-block:: csharp

    public class FakeHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //do stuff and optionally call the base handler..
            return await base.SendAsync(request, cancellationToken);
        }
    }

Next you must add the handlers to Ocelot's container like below...

.. code-block:: csharp

    services.AddOcelot()
            .AddDelegatingHandler<FakeHandler>()
            .AddDelegatingHandler<FakeHandlerTwo>()

Both of these Add methods have a default parameter called global which is set to false. If it is false then the intent of the DelegatingHandler is to be applied to specific Routes via ocelot.json (more on that later). If it is set to true
then it becomes a global handler and will be applied to all Routes.

e.g.

As below...

.. code-block:: csharp

    services.AddOcelot()
            .AddDelegatingHandler<FakeHandler>(true)

Finally if you want Route specific DelegatingHandlers or to order your specific and / or global (more on this later) DelegatingHandlers then you must add the following json to the specific Route in ocelot.json. The names in the array must match the class names of your
DelegatingHandlers for Ocelot to match them together.

.. code-block:: json

    "DelegatingHandlers": [
        "FakeHandlerTwo",
        "FakeHandler"
    ]

You can have as many DelegatingHandlers as you want and they are run in the following order:

1. Any globals that are left in the order they were added to services and are not in the DelegatingHandlers array from ocelot.json.
2. Any non global DelegatingHandlers plus any globals that were in the DelegatingHandlers array from ocelot.json ordered as they are in the DelegatingHandlers array.
3. Tracing DelegatingHandler if enabled (see tracing docs).
4. QoS DelegatingHandler if enabled (see QoS docs).
5. The HttpClient sends the HttpRequestMessage.

Access to HttpContext
^^^^^^^^^^^^^^^^^^^^^

If you need a HttpContext instance in your delegating handler, don't use HttpContextAccessor to obtain it (it will not contain all the information, for example, it will not contain authenticated user data or claims). Instead, implement IDelegatingHandlerWithHttpContext interface, then a valid HttpContext will appear as a property of the delegating handler class.

Hopefully other people will find this feature useful!
