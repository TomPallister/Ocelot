Rate Limiting
=============

Thanks to `@catcherwong article <http://www.c-sharpcorner.com/article/building-api-gateway-using-ocelot-in-asp-net-core-rate-limiting-part-four/>`_ for inspiring me to finally write this documentation.

Ocelot supports rate limiting of upstream requests so that your downstream services do not become overloaded. This feature was added by @geffzhang on GitHub! Thanks very much.

OK so to get rate limiting working for a Route you need to add the following json to it. 

.. code-block:: json

    "RateLimitOptions": {  
        "ClientWhitelist": [],  
        "EnableRateLimiting": true,  
        "Period": "1s",  
        "PeriodTimespan": 1,  
        "Limit": 1  
    }  

ClientWhitelist - This is an array that contains the whitelist of the client. It means that the client in this array will not be affected by the rate limiting.

EnableRateLimiting - This value specifies enable endpoint rate limiting.

Period - This value specifies the period that the limit applies to, such as 1s, 5m, 1h,1d and so on. If you make more requests in the period than the limit allows then you need to wait for PeriodTimespan to elapse before you make another request.

PeriodTimespan - This value specifies that we can retry after a certain number of seconds.

Limit - This value specifies the maximum number of requests that a client can make in a defined period.

You can also set the following in the GlobalConfiguration part of ocelot.json

.. code-block:: json

    "RateLimitOptions": {  
      "DisableRateLimitHeaders": false,  
      "QuotaExceededMessage": "Customize Tips!",  
      "HttpStatusCode": 999,
      "ClientIdHeader" : "Test"
    }  

DisableRateLimitHeaders - This value specifies whether X-Rate-Limit and Retry-After headers are disabled.

QuotaExceededMessage - This value specifies the exceeded message.

HttpStatusCode - This value specifies the returned HTTP Status code when rate limiting occurs.

ClientIdHeader - Allows you to specifiy the header that should be used to identify clients. By default it is "ClientId"
