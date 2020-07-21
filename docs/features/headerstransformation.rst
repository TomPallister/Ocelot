Headers Transformation
======================

Ocelot allows the user to transform headers pre and post downstream request. At the moment Ocelot only supports find and replace. This feature was requested `GitHub #190 <https://github.com/ThreeMammals/Ocelot/issues/190>`_ and I decided that it was going to be useful in various ways.

Add to Request
^^^^^^^^^^^^^^

This feature was requestes in `GitHub #313 <https://github.com/ThreeMammals/Ocelot/issues/313>`_.

If you want to add a header to your upstream request please add the following to a Route in your ocelot.json:

.. code-block:: json

    "UpstreamHeaderTransform": {
        "Uncle": "Bob"
    }

In the example above a header with the key Uncle and value Bob would be send to to the upstream service.

Placeholders are supported too (see below).

Add to Response
^^^^^^^^^^^^^^^

This feature was requested in `GitHub #280 <https://github.com/ThreeMammals/Ocelot/issues/280>`_.

If you want to add a header to your downstream response please add the following to a Route in ocelot.json..

.. code-block:: json

    "DownstreamHeaderTransform": {
        "Uncle": "Bob"
    },

In the example above a header with the key Uncle and value Bob would be returned by Ocelot when requesting the specific Route.

If you want to return the Butterfly APM trace id then do something like the following..

.. code-block:: json

    "DownstreamHeaderTransform": {
        "AnyKey": "{TraceId}"
    },

Find and Replace
^^^^^^^^^^^^^^^^

In order to transform a header first we specify the header key and then the type of transform we want e.g.

.. code-block:: json

    "Test": "http://www.bbc.co.uk/, http://ocelot.com/"

The key is "Test" and the value is "http://www.bbc.co.uk/, http://ocelot.com/". The value is saying replace http://www.bbc.co.uk/ with http://ocelot.com/. The syntax is {find}, {replace}. Hopefully pretty simple. There are examples below that explain more.

Pre Downstream Request
^^^^^^^^^^^^^^^^^^^^^^

Add the following to a Route in ocelot.json in order to replace http://www.bbc.co.uk/ with http://ocelot.com/. This header will be changed before the request downstream and will be sent to the downstream server.

.. code-block:: json

     "UpstreamHeaderTransform": {
        "Test": "http://www.bbc.co.uk/, http://ocelot.com/"
    },

Post Downstream Request
^^^^^^^^^^^^^^^^^^^^^^^

Add the following to a Route in ocelot.json in order to replace http://www.bbc.co.uk/ with http://ocelot.com/. This transformation will take place after Ocelot has received the response from the downstream service.

.. code-block:: json

    "DownstreamHeaderTransform": {
        "Test": "http://www.bbc.co.uk/, http://ocelot.com/"
    },

Placeholders
^^^^^^^^^^^^

Ocelot allows placeholders that can be used in header transformation.

{RemoteIpAddress} - This will find the clients IP address using _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString() so you will get back some IP.
{BaseUrl} - This will use Ocelot's base url e.g. http://localhost:5000 as its value.
{DownstreamBaseUrl} - This will use the downstream services base url e.g. http://localhost:5000 as its value. This only works for DownstreamHeaderTransform at the moment.
{TraceId} - This will use the Butterfly APM Trace Id. This only works for DownstreamHeaderTransform at the moment.
{UpstreamHost} - This will look for the incoming Host header.

Handling 302 Redirects
^^^^^^^^^^^^^^^^^^^^^^
Ocelot will by default automatically follow redirects however if you want to return the location header to the client you might want to change the location to be Ocelot not the downstream service. Ocelot allows this with the following configuration.

.. code-block:: json

    "DownstreamHeaderTransform": {
        "Location": "http://www.bbc.co.uk/, http://ocelot.com/"
    },
     "HttpHandlerOptions": {
        "AllowAutoRedirect": false,
    },

or you could use the BaseUrl placeholder.

.. code-block:: json

    "DownstreamHeaderTransform": {
        "Location": "http://localhost:6773, {BaseUrl}"
    },
     "HttpHandlerOptions": {
        "AllowAutoRedirect": false,
    },

finally if you are using a load balancer with Ocelot you will get multiple downstream base urls so the above would not work. In this case you can do the following.

.. code-block:: json

    "DownstreamHeaderTransform": {
        "Location": "{DownstreamBaseUrl}, {BaseUrl}"
    },
     "HttpHandlerOptions": {
        "AllowAutoRedirect": false,
    },

X-Forwarded-For
^^^^^^^^^^^^^^^

An example of using {RemoteIpAddress} placeholder...

.. code-block:: json

  "UpstreamHeaderTransform": {
        "X-Forwarded-For": "{RemoteIpAddress}"
    }

Future
^^^^^^

Ideally this feature would be able to support the fact that a header can have multiple values. At the moment it just assumes one. It would also be nice if it could multi find and replace e.g. 

.. code-block:: json

    "DownstreamHeaderTransform": {
        "Location": "[{one,one},{two,two}"
    },
     "HttpHandlerOptions": {
        "AllowAutoRedirect": false,
    },

If anyone wants to have a go at this please help yourself!!
