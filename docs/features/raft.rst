Raft (EXPERIMENTAL DO NOT USE IN PRODUCTION)
============================================

Ocelot has recently integrated `Rafty <https://github.com/ThreeMammals/Rafty>`_ which is an implementation of Raft that I have also been working on over the last year. This project is very experimental so please do not use this feature of Ocelot in production until I think it's OK.

Raft is a distributed concensus algorithm that allows a cluster of servers (Ocelots) to maintain local state without having a centralised database for storing state (e.g. SQL Server). 

To get Raft support you must first install the Ocelot Rafty package.

``Install-Package Ocelot.Provider.Rafty``

Then you must make the following changes to your Startup.cs / Program.cs.

.. code-block:: csharp

    public virtual void ConfigureServices(IServiceCollection services)
    {
         services
            .AddOcelot()
            .AddAdministration("/administration", "secret")
            .AddRafty();
    }

In addition to this you must add a file called peers.json to your main project and it will look as follows

.. code-block:: json

    {
        "Peers": [{
                "HostAndPort": "http://localhost:5000"
            },
            {
                "HostAndPort": "http://localhost:5002"
            },
            {
                "HostAndPort": "http://localhost:5003"
            },
            {
                "HostAndPort": "http://localhost:5004"
            },
            {
                "HostAndPort": "http://localhost:5001"
            }
        ]
    }

Each instance of Ocelot must have it's address in the array so that they can communicate using Rafty.

Once you have made these configuration changes you must deploy and start each instance of Ocelot using the addresses in the peers.json file. The servers should then start communicating with each other! You can test if everything is working by posting a configuration update and checking it has replicated to all servers by getting their configuration.
