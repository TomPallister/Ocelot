Logging
=======

Ocelot uses the standard logging interfaces ILoggerFactory / ILogger<T> at the moment. This is encapsulated in  IOcelotLogger / IOcelotLoggerFactory with an implementation 
for the standard asp.net core logging stuff at the moment. This is because Ocelot add's some extra info to the logs such as request id if it is configured.

There is a global error handler that should catch any exceptions thrown and log them as errors.

Finally if logging is set to trace level Ocelot will log starting, finishing and any middlewares that throw an exception which can be quite useful.

The reason for not just using bog standard framework logging is that I could not work out how to override the request id that get's logged when setting IncludeScopes 
to true for logging settings. Nicely onto the next feature.

Warning
^^^^^^^

If you are logging to Console you will get terrible performance. I have had so many issues about performance issues with Ocelot and it is always logging level Debug, logging to Console :) Make sure you are logging to something proper in production :)
