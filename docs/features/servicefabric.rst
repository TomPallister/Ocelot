Service Fabric
==============

If you have services deployed in Service Fabric you will normally use the naming service to access them.

The following example shows how to set up a Route that will work in Service Fabric. The most important thing is the ServiceName which is made up of the Service Fabric application name then the specific service name. We also need to set up the ServiceDiscoveryProvider in  GlobalConfiguration. The example here shows a typical configuration. It assumes service fabric is running on localhost and that the naming service is on port 19081.

The example below is taken from the samples folder so please check it if this doesnt make sense!

.. code-block:: json

    {
        "Routes": [
            {
            "DownstreamPathTemplate": "/api/values",
            "UpstreamPathTemplate": "/EquipmentInterfaces",
            "UpstreamHttpMethod": [
                "Get"
            ],
            "DownstreamScheme": "http",
            "ServiceName": "OcelotServiceApplication/OcelotApplicationService",
            }
        ],
        "GlobalConfiguration": {
            "RequestIdKey": "OcRequestId",
            "ServiceDiscoveryProvider": {
                "Host": "localhost",
                "Port": 19081,
                "Type": "ServiceFabric"
            }
        }
    }

If you are using stateless / guest exe services ocelot will be able to proxy through the naming service without anything else. However
if you are using statefull / actor services you must send the PartitionKind and PartitionKey query string values with the client
request e.g.

GET http://ocelot.com/EquipmentInterfaces?PartitionKind=xxx&PartitionKey=xxx

There is no way for Ocelot to work these out for you. 
