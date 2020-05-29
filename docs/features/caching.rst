Caching
=======

Ocelot supports some very rudimentary caching at the moment provider by the `CacheManager <https://github.com/MichaCo/CacheManager>`_ project. This is an amazing project that is solving a lot of caching problems. I would reccomend using this package to cache with Ocelot. 

The following example shows how to add CacheManager to Ocelot so that you can do output caching. 

First of all add the following NuGet package.

   ``Install-Package Ocelot.Cache.CacheManager``

This will give you access to the Ocelot cache manager extension methods.

The second thing you need to do something like the following to your ConfigureServices..

.. code-block:: csharp

    s.AddOcelot()
        .AddCacheManager(x =>
        {
            x.WithDictionaryHandle();
        })

Finally in order to use caching on a route in your Route configuration add this setting.

.. code-block:: json

    "FileCacheOptions": { "TtlSeconds": 15, "Region": "somename" }

In this example ttl seconds is set to 15 which means the cache will expire after 15 seconds.

If you look at the example `here <https://github.com/ThreeMammals/Ocelot/blob/master/test/Ocelot.ManualTest/Program.cs>`_ you can see how the cache manager is setup and then passed into the Ocelot AddCacheManager configuration method. You can use any settings supported by the CacheManager package and just pass them in.

Anyway Ocelot currently supports caching on the URL of the downstream service and setting a TTL in seconds to expire the cache. You can also clear the cache for a region by calling Ocelot's administration API.

Your own caching
^^^^^^^^^^^^^^^^

If you want to add your own caching method implement the following interfaces and register them in DI e.g. ``services.AddSingleton<IOcelotCache<CachedResponse>, MyCache>()``

``IOcelotCache<CachedResponse>`` this is for output caching.

``IOcelotCache<FileConfiguration>`` this is for caching the file configuration if you are calling something remote to get your config such as Consul.

Please dig into the Ocelot source code to find more. I would really appreciate it if anyone wants to implement Redis, memcache etc..

