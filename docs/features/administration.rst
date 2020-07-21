Administration
==============

Ocelot supports changing configuration during runtime via an authenticated HTTP API. This can be authenticated in two ways either using Ocelot's internal IdentityServer (for authenticating requests to the administration API only) or hooking the administration API authentication into your own IdentityServer.

The first thing you need to do if you want to use the administration API is bring in the relavent NuGet package..

``Install-Package Ocelot.Administration``

This will bring down everything needed by the admin API.

Providing your own IdentityServer
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^



All you need to do to hook into your own IdentityServer is add the following to your ConfigureServices method.

.. code-block:: csharp

    public virtual void ConfigureServices(IServiceCollection services)
    {
        Action<IdentityServerAuthenticationOptions> options = o => {
                // o.Authority = ;
                // o.ApiName = ;
                // etc....
            };

        services
            .AddOcelot()
            .AddAdministration("/administration", options);
    }

You now need to get a token from your IdentityServer and use in subsequent requests to Ocelot's administration API.

This feature was implemented for `issue 228 <https://github.com/ThreeMammals/Ocelot/issues/228>`_. It is useful because the IdentityServer authentication middleware needs the URL of the IdentityServer. If you are using the internal IdentityServer it might not alaways be possible to have the Ocelot URL.  

Internal IdentityServer
^^^^^^^^^^^^^^^^^^^^^^^

The API is authenticated using bearer tokens that you request from Ocelot iteself. This is provided by the amazing `Identity Server <https://github.com/IdentityServer/IdentityServer4>`_ project that I have been using for a few years now. Check them out.

In order to enable the administration section you need to do a few things. First of all add this to yourinitial Startup.cs. 

The path can be anything you want and it is obviously reccomended don't usea url you would like to route through with Ocelot as this will not work. The administration uses theMapWhen functionality of asp.net core and all requests to {root}/administration will be sent there not to the Ocelot middleware.

The secret is the client secret that Ocelot's internal IdentityServer will use to authenticate requests to the administration API. This can be whatever you want it to be!

.. code-block:: csharp

    public virtual void ConfigureServices(IServiceCollection services)
    {
        services
            .AddOcelot()
            .AddAdministration("/administration", "secret");
    }

In order for the administration API to work, Ocelot / IdentityServer must be able to call itself for validation. This means that you need to add the base url of Ocelot to global configuration if it is not default (http://localhost:5000). Please note if you are using something like docker to host Ocelot it might not be able to call back to localhost etc and you need to know what you are doing with docker networking in this scenario. Anyway this can be done as follows..

If you want to run on a different host and port locally..

.. code-block:: json

 "GlobalConfiguration": {
    "BaseUrl": "http://localhost:55580"
  }

or if Ocelot is exposed via dns

.. code-block:: json

 "GlobalConfiguration": {
    "BaseUrl": "http://mydns.com"
  }

Now if you went with the configuration options above and want to access the API you can use the postman scriptscalled ocelot.postman_collection.json in the solution to change the Ocelot configuration. Obviously these will need to be changed if you are running Ocelot on a different url to http://localhost:5000.


The scripts show you how to request a bearer token from ocelot and then use it to GET the existing configuration and POST a configuration.

If you are running multiple Ocelot instances in a cluster then you need to use a certificate to sign the bearer tokens used to access the administration API.

In order to do this you need to add two more environmental variables for each Ocelot in the cluster.

``OCELOT_CERTIFICATE``
    The path to a certificate that can be used to sign the tokens. The certificate needs to be of the type X509 and obviously Ocelot needs to be able to access it.
``OCELOT_CERTIFICATE_PASSWORD``
    The password for the certificate.

Normally Ocelot just uses temporary signing credentials but if you set these environmental variables then it will use the certificate. If all the other Ocelot instances in thecluster have the same certificate then you are good!


Administration API
^^^^^^^^^^^^^^^^^^

**POST {adminPath}/connect/token**

This gets a token for use with the admin area using the client credentials we talk about setting above. Under the hood this calls into an IdentityServer hosted within Ocelot.

The body of the request is form-data as follows

``client_id`` set as admin

``client_secret`` set as whatever you used when setting up the administration services.

``scope`` set as admin

``grant_type`` set as client_credentials

**GET {adminPath}/configuration**


This gets the current Ocelot configuration. It is exactly the same JSON we use to set Ocelot up with in the first place.

**POST {adminPath}/configuration**

This overrwrites the existing configuration (should probably be a put!). I reccomend getting your config from the GET endpoint, making any changes and posting it back...simples.

The body of the request is JSON and it is the same format as the FileConfiguration.cs that we use to set up 
Ocelot on a file system. 

Please note that if you want to use this API then the process running Ocelot must have permission to write to the disk where your ocelot.json or ocelot.{environment}.json is located. This is because Ocelot will overwrite them on save. 

**DELETE {adminPath}/outputcache/{region}**

This clears a region of the cache. If you are using a backplane it will clear all instances of the cache! Giving your the ability to run a cluster of Ocelots and cache over all of them in memory and clear them all at the same time / just use a distributed cache.

The region is whatever you set against the Region field in the FileCacheOptions section of the Ocelot configuration.
