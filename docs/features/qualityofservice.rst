Quality of Service
==================

Ocelot supports one QoS capability at the current time. You can set on a per Route basis if you want to use a circuit breaker when making requests to a downstream service. This uses an awesome
.NET library called Polly check them out `here <https://github.com/App-vNext/Polly>`_.

The first thing you need to do if you want to use the administration API is bring in the relevant NuGet package..

``Install-Package Ocelot.Provider.Polly``

Then in your ConfigureServices method

.. code-block:: csharp

    public virtual void ConfigureServices(IServiceCollection services)
    {
        services
            .AddOcelot()
            .AddPolly();
    }

Then add the following section to a Route configuration. 

.. code-block:: json

    "QoSOptions": {
        "ExceptionsAllowedBeforeBreaking":3,
        "DurationOfBreak":1000,
        "TimeoutValue":5000
    }

You must set a number greater than 0 against ExceptionsAllowedBeforeBreaking for this rule to be implemented. Duration of break means the circuit breaker will stay open for 1 second after it is tripped.
TimeoutValue means if a request takes more than 5 seconds it will automatically be timed out. 

You can set the TimeoutValue in isolation of the ExceptionsAllowedBeforeBreaking and DurationOfBreak options. 

.. code-block:: json

    "QoSOptions": {
        "TimeoutValue":5000
    }

There is no point setting the other two in isolation as they affect each other :)

If you do not add a QoS section QoS will not be used however Ocelot will default to a 90 second timeout on all downstream requests. If someone needs this to be configurable open an issue.
