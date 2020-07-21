Middleware Injection and Overrides
==================================

Warning use with caution. If you are seeing any exceptions or strange behavior in your middleware 
pipeline and you are using any of the following. Remove them and try again!

When setting up Ocelot in your Startup.cs you can provide some additional middleware 
and override middleware. This is done as follows.

.. code-block:: csharp

    var configuration = new OcelotPipelineConfiguration
    {
        PreErrorResponderMiddleware = async (ctx, next) =>
        {
            await next.Invoke();
        }
    };

    app.UseOcelot(configuration);

In the example above the provided function will run before the first piece of Ocelot middleware. 
This allows a user to supply any behaviors they want before and after the Ocelot pipeline has run.
This means you can break everything so use at your own pleasure!

The user can set functions against the following.

* PreErrorResponderMiddleware - Already explained above.

* PreAuthenticationMiddleware - This allows the user to run pre authentication logic and then call Ocelot's authentication middleware.

* AuthenticationMiddleware - This overrides Ocelots authentication middleware.

* PreAuthorisationMiddleware - This allows the user to run pre authorisation logic and then call Ocelot's authorisation middleware.

* AuthorisationMiddleware - This overrides Ocelots authorisation middleware.

* PreQueryStringBuilderMiddleware - This allows the user to manipulate the query string on the http request before it is passed to Ocelots request creator.

Obviously you can just add middleware as normal before the call to app.UseOcelot() It cannot be added
after as Ocelot does not call the next middleware.
