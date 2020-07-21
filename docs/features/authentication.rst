Authentication
==============

In order to authenticate Routes and subsequently use any of Ocelot's claims based features such as authorisation or modifying the request with values from the token. Users must register authentication services in their Startup.cs as usual but they provide a scheme (authentication provider key) with each registration e.g.

.. code-block:: csharp

    public void ConfigureServices(IServiceCollection services)
    {
        var authenticationProviderKey = "TestKey";

        services.AddAuthentication()
            .AddJwtBearer(authenticationProviderKey, x =>
            {
            });
    }


In this example TestKey is the scheme that this provider has been registered with. We then map this to a Route in the configuration e.g.

.. code-block:: json

    "Routes": [{
            "DownstreamHostAndPorts": [
                {
                    "Host": "localhost",
                    "Port": 51876,
                }
            ],
            "DownstreamPathTemplate": "/",
            "UpstreamPathTemplate": "/",
            "UpstreamHttpMethod": ["Post"],
            "RouteIsCaseSensitive": false,
            "DownstreamScheme": "http",
            "AuthenticationOptions": {
                "AuthenticationProviderKey": "TestKey",
                "AllowedScopes": []
            }
        }]

When Ocelot runs it will look at this Routes AuthenticationOptions.AuthenticationProviderKey and check that there is an Authentication provider registered with the given key. If there isn't then Ocelot will not start up, if there is then the Route will use that provider when it executes.

If a Route is authenticated Ocelot will invoke whatever scheme is associated with it while executing the authentication middleware. If the request fails authentication Ocelot returns a http status code 401.

JWT Tokens
^^^^^^^^^^

If you want to authenticate using JWT tokens maybe from a provider like Auth0 you can register your authentication middleware as normal e.g.

.. code-block:: csharp

    public void ConfigureServices(IServiceCollection services)
    {
        var authenticationProviderKey = "TestKey";
        
        services.AddAuthentication()
            .AddJwtBearer(authenticationProviderKey, x =>
            {
                x.Authority = "test";
                x.Audience = "test";
            });

        services.AddOcelot();
    }

Then map the authentication provider key to a Route in your configuration e.g.

.. code-block:: json

    "Routes": [{
            "DownstreamHostAndPorts": [
                {
                    "Host": "localhost",
                    "Port": 51876,
                }
            ],
            "DownstreamPathTemplate": "/",
            "UpstreamPathTemplate": "/",
            "UpstreamHttpMethod": ["Post"],
            "RouteIsCaseSensitive": false,
            "DownstreamScheme": "http",
            "AuthenticationOptions": {
                "AuthenticationProviderKey": "TestKey",
                "AllowedScopes": []
            }
        }]



Identity Server Bearer Tokens
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

In order to use IdentityServer bearer tokens, register your IdentityServer services as usual in ConfigureServices with a scheme (key). If you don't understand how to do this please consult the IdentityServer documentation.

.. code-block:: csharp

    public void ConfigureServices(IServiceCollection services)
    {
        var authenticationProviderKey = "TestKey";
        Action<IdentityServerAuthenticationOptions> options = o =>
            {
                o.Authority = "https://whereyouridentityserverlives.com";
                o.ApiName = "api";
                o.SupportedTokens = SupportedTokens.Both;
                o.ApiSecret = "secret";
            };

        services.AddAuthentication()
            .AddIdentityServerAuthentication(authenticationProviderKey, options);

        services.AddOcelot();
    }

Then map the authentication provider key to a Route in your configuration e.g.

.. code-block:: json

    "Routes": [{
            "DownstreamHostAndPorts": [
                {
                    "Host": "localhost",
                    "Port": 51876,
                }
            ],
            "DownstreamPathTemplate": "/",
            "UpstreamPathTemplate": "/",
            "UpstreamHttpMethod": ["Post"],
            "RouteIsCaseSensitive": false,
            "DownstreamScheme": "http",
            "AuthenticationOptions": {
                "AuthenticationProviderKey": "TestKey",
                "AllowedScopes": []
            }
        }]

Okta
^^^^
Add the following to your startup Configure method:

.. code-block:: csharp

    app
        .UseAuthentication()
        .UseOcelot()
        .Wait();


Add the following, at minimum, to your startup ConfigureServices method:

.. code-block:: csharp

     services
         .AddAuthentication()
         .AddJwtBearer(oktaProviderKey, options =>
         {
             options.Audience = configuration["Authentication:Okta:Audience"]; // Okta Authorization server Audience
             options.Authority = configuration["Authentication:Okta:Server"]; // Okta Authorization Issuer URI URL e.g. https://{subdomain}.okta.com/oauth2/{authidentifier}
         });
    services.AddOcelot(configuration);


NOTE: In order to get Ocelot to view the scope claim from Okta properly, you have to add the following to map the default Okta "scp" claim to "scope"


.. code-block:: csharp

     // Map Okta scp to scope claims instead of http://schemas.microsoft.com/identity/claims/scope to allow ocelot to read/verify them
     JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("scp");
     JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Add("scp", "scope");


`Issue 446 <https://github.com/ThreeMammals/Ocelot/issues/446>`_ that contains some code and examples that might help with Okta integration.

Allowed Scopes
^^^^^^^^^^^^^

If you add scopes to AllowedScopes Ocelot will get all the user claims (from the token) of the type scope and make sure that the user has all of the scopes in the list.

This is a way to restrict access to a Route on a per scope basis.
