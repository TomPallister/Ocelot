Authorization
=============

Ocelot supports claims based authorization which is run post authentication. This means if you have a route you want to authorize you can add the following to you Route configuration.

.. code-block:: json

    "RouteClaimsRequirement": {
        "UserType": "registered"
    }

In this example when the authorization middleware is called Ocelot will check to seeif the user has the claim type UserType and if the value of that claim is registered. If it isn't then the user will not be authorized and the response will be 403 forbidden.



