Configuration
============

An example configuration can be found `here <https://github.com/ThreeMammals/Ocelot/blob/master/test/Ocelot.ManualTest/ocelot.json>`_.
There are two sections to the configuration. An array of ReRoutes and a GlobalConfiguration. 
The ReRoutes are the objects that tell Ocelot how to treat an upstream request. The Global 
configuration is a bit hacky and allows overrides of ReRoute specific settings. It's useful
if you don't want to manage lots of ReRoute specific settings.

.. code-block:: json

    {
        "ReRoutes": [],
        "GlobalConfiguration": {}
    }

Here is an example ReRoute configuration, You don't need to set all of these things but this is everything that is available at the moment:

.. code-block:: json

  {
            "DownstreamPathTemplate": "/",
            "UpstreamPathTemplate": "/",
            "UpstreamHttpMethod": [
                "Get"
            ],
            "DownstreamHttpMethod": "",
            "DownstreamHttpVersion": "",
            "AddHeadersToRequest": {},
            "AddClaimsToRequest": {},
            "RouteClaimsRequirement": {},
            "AddQueriesToRequest": {},
            "RequestIdKey": "",
            "FileCacheOptions": {
                "TtlSeconds": 0,
                "Region": ""
            },
            "ReRouteIsCaseSensitive": false,
            "ServiceName": "",
            "DownstreamScheme": "http",
            "DownstreamHostAndPorts": [
                {
                    "Host": "localhost",
                    "Port": 51876,
                }
            ],
            "QoSOptions": {
                "ExceptionsAllowedBeforeBreaking": 0,
                "DurationOfBreak": 0,
                "TimeoutValue": 0
            },
            "LoadBalancer": "",
            "RateLimitOptions": {
                "ClientWhitelist": [],
                "EnableRateLimiting": false,
                "Period": "",
                "PeriodTimespan": 0,
                "Limit": 0
            },
            "AuthenticationOptions": {
                "AuthenticationProviderKey": "",
                "AllowedScopes": []
            },
            "HttpHandlerOptions": {
                "AllowAutoRedirect": true,
                "UseCookieContainer": true,
                "UseTracing": true,
                "MaxConnectionsPerServer": 100
            },
            "DangerousAcceptAnyServerCertificateValidator": false
        }

More information on how to use these options is below..

Multiple environments
^^^^^^^^^^^^^^^^^^^^^

Like any other asp.net core project Ocelot supports configuration file names such as configuration.dev.json, configuration.test.json etc. In order to implement this add the following 
to you 

.. code-block:: csharp

        .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config
                        .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                        .AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, true)
                        .AddJsonFile("ocelot.json")
                        .AddJsonFile($"configuration.{hostingContext.HostingEnvironment.EnvironmentName}.json")
                        .AddEnvironmentVariables();
                })

Ocelot will now use the environment specific configuration and fall back to ocelot.json if there isn't one.

You also need to set the corresponding environment variable which is ASPNETCORE_ENVIRONMENT. More info on this can be found in the `asp.net core docs <https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments>`_.

Merging configuration files
^^^^^^^^^^^^^^^^^^^^^^^^^^^

This feature was requested in `Issue 296 <https://github.com/ThreeMammals/Ocelot/issues/296>`_ and allows users to have multiple configuration files to make managing large configurations easier.

Instead of adding the configuration directly e.g. AddJsonFile("ocelot.json") you can call AddOcelot() like below. 

.. code-block:: csharp

    .ConfigureAppConfiguration((hostingContext, config) =>
        {
            config
                .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, true)
                .AddOcelot(hostingContext.HostingEnvironment)
                .AddEnvironmentVariables();
        })

In this scenario Ocelot will look for any files that match the pattern (?i)ocelot.([a-zA-Z0-9]*).json and then merge these together. If you want to set the GlobalConfiguration property you must have a file called ocelot.global.json. 

The way Ocelot merges the files is basically load them, loop over them, add any ReRoutes, add any AggregateReRoutes and if the file is called ocelot.global.json add the GlobalConfiguration aswell as any ReRoutes or AggregateReRoutes. Ocelot will then save the merged configuration to a file called ocelot.json and this will be used as the source of truth while ocelot is running.

At the moment there is no validation at this stage it only happens when Ocelot validates the final merged configuration. This is something to be aware of when you are investigating problems. I would advise always checking what is in ocelot.json if you have any problems.

You can also give Ocelot a specific path to look in for the configuration files like below.

.. code-block:: csharp

    .ConfigureAppConfiguration((hostingContext, config) =>
        {
            config
                .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, true)
                .AddOcelot("/foo/bar", hostingContext.HostingEnvironment)
                .AddEnvironmentVariables();
        })

Ocelot needs the HostingEnvironment so it knows to exclude anything environment specific from the algorithm. 

Merging configuration files to memory only
------------------------------------------

By default Ocelot writes the merged configuration back to disk as "ocelot.json" and add that file to config. If your server don't have write permissions to your configuration folder, you can instruct Ocelot to use the merged configuration directly from memory instead.

.. code-block:: csharp

    config.AddOcelot(hostingContext.HostingEnvironment, MergeOcelotJson.ToMemory)

Store configuration in consul
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

The first thing you need to do is install the NuGet package that provides Consul support in Ocelot.

``Install-Package Ocelot.Provider.Consul``

Then you add the following when you register your services Ocelot will attempt to store and retrieve its configuration in consul KV store.

.. code-block:: csharp

 services
    .AddOcelot()
    .AddConsul()
    .AddConfigStoredInConsul();

You also need to add the following to your ocelot.json. This is how Ocelot
finds your Consul agent and interacts to load and store the configuration from Consul.

.. code-block:: json

    "GlobalConfiguration": {
        "ServiceDiscoveryProvider": {
            "Host": "localhost",
            "Port": 9500
        }
    }

I decided to create this feature after working on the Raft consensus algorithm and finding out its super hard. Why not take advantage of the fact Consul already gives you this! 
I guess it means if you want to use Ocelot to its fullest you take on Consul as a dependency for now.

This feature has a 3 second ttl cache before making a new request to your local consul agent.

Reload JSON config on change
^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Ocelot supports reloading the json configuration file on change. e.g. the following will recreate Ocelots internal configuration when the ocelot.json file is updated
manually.

.. code-block:: json

    config.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

Configuration Key
-----------------

If you are using Consul for configuration (or other providers in the future) you might want to key your configurations so you can have multiple configurations :) This feature was requested in `issue 346 <https://github.com/ThreeMammals/Ocelot/issues/346>`_! In order to specify the key you need to set the ConfigurationKey property in the ServiceDiscoveryProvider section of the configuration json file e.g.

.. code-block:: json

    "GlobalConfiguration": {
        "ServiceDiscoveryProvider": {
            "Host": "localhost",
            "Port": 9500,
            "ConfigurationKey": "Oceolot_A"
        }
    }

In this example Ocelot will use Oceolot_A as the key for your configuration when looking it up in Consul.

If you do not set the ConfigurationKey Ocelot will use the string InternalConfiguration as the key.

Follow Redirects / Use CookieContainer 
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Use HttpHandlerOptions in ReRoute configuration to set up HttpHandler behavior:

1. AllowAutoRedirect is a value that indicates whether the request should follow redirection responses. Set it true if the request should automatically 
follow redirection responses from the Downstream resource; otherwise false. The default value is false.

2. UseCookieContainer is a value that indicates whether the handler uses the CookieContainer 
property to store server cookies and uses these cookies when sending requests. The default value is false. Please note
that if you are using the CookieContainer Ocelot caches the HttpClient for each downstream service. This means that all requests
to that DownstreamService will share the same cookies. `Issue 274 <https://github.com/ThreeMammals/Ocelot/issues/274>`_ was created because a user
noticed that the cookies were being shared. I tried to think of a nice way to handle this but I think it is impossible. If you don't cache the clients
that means each request gets a new client and therefore a new cookie container. If you clear the cookies from the cached client container you get race conditions due to inflight
requests. This would also mean that subsequent requests don't use the cookies from the previous response! All in all not a great situation. I would avoid setting 
UseCookieContainer to true unless you have a really really good reason. Just look at your response headers and forward the cookies back with your next request! 

SSL Errors
^^^^^^^^^^

If you want to ignore SSL warnings / errors set the following in your ReRoute config.

.. code-block:: json

    "DangerousAcceptAnyServerCertificateValidator": true

I don't recommend doing this, I suggest creating your own certificate and then getting it trusted by your local / remote machine if you can.

MaxConnectionsPerServer
^^^^^^^^^^^^^^^^^^^^^^^

This controls how many connections the internal HttpClient will open. This can be set at ReRoute or global level.

React to Configuration Changes
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Resolve IOcelotConfigurationChangeTokenSource from the DI container if you wish to react to changes to the Ocelot configuration via the Ocelot.Administration API or ocelot.json being reloaded from the disk. You may either poll the change token's HasChanged property, or register a callback with the RegisterChangeCallback method.

Polling the HasChanged property
-------------------------------

.. code-block:: csharp
    public class ConfigurationNotifyingService : BackgroundService
    {
        private readonly IOcelotConfigurationChangeTokenSource _tokenSource;
        private readonly ILogger _logger;
        public ConfigurationNotifyingService(IOcelotConfigurationChangeTokenSource tokenSource, ILogger logger)
        {
            _tokenSource = tokenSource;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_tokenSource.ChangeToken.HasChanged)
                {
                    _logger.LogInformation("Configuration updated");
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
    
Registering a callback
----------------------

.. code-block:: csharp
    public class MyDependencyInjectedClass : IDisposable
    {
        private readonly IOcelotConfigurationChangeTokenSource _tokenSource;
        private readonly IDisposable _callbackHolder;
        public MyClass(IOcelotConfigurationChangeTokenSource tokenSource)
        {
            _tokenSource    = tokenSource;
            _callbackHolder = tokenSource.ChangeToken.RegisterChangeCallback(_ => Console.WriteLine("Configuration changed"), null);
        }
        public void Dispose()
        {
            _callbackHolder.Dispose();
        }
    }

DownstreamHttpVersion
---------------------

Ocelot allows you to choose the HTTP version it will use to make the proxy request. It can be set as "1.0", "1.1" or "2.0".