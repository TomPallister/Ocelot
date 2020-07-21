# Ocelot using GraphQL example

Loads of people keep asking me if Ocelot will every support GraphQL, in my mind Ocelot and GraphQL are two different things that can work together. 
I would not try and implement GraphQL in Ocelot instead I would either have Ocelot in front of GraphQL to handle things like authorisation / authentication or I would 
bring in the awesome [graphql-dotnet](https://github.com/graphql-dotnet/graphql-dotnet) library and use it in a [DelegatingHandler](http://ocelot.readthedocs.io/en/latest/features/delegatinghandlers.html). This way you could have Ocelot and GraphQL without the extra hop to GraphQL. This same is an example of how to do that. 

## Example

If you run this project with

$ dotnet run

Use postman or something to make the following requests and you can see Ocelot and GraphQL in action together...

GET http://localhost:5000/graphql?query={ hero(id: 4) { id name } }

RESPONSE
```json
    {
        "data": {
            "hero": {
            "id": 4,
            "name": "Tom Pallister"
            }
        }
    }
```

POST http://localhost:5000/graphql

BODY
```json
    { hero(id: 4) { id name } }
```

RESPONSE
```json
    {
        "data": {
            "hero": {
            "id": 4,
            "name": "Tom Pallister"
            }
        }
    }
```

## Notes

Please note this project never goes out to another service, it just gets the data for GraphQL in memory. You would need to add the details of your GraphQL server in ocelot.json e.g.

```json
{
    "Routes": [
        {
            "DownstreamPathTemplate": "/graphql",
            "DownstreamScheme": "http",
            "DownstreamHostAndPorts": [
              {
                "Host": "yourgraphqlhost.com",
                "Port": 80
              }
            ],
            "UpstreamPathTemplate": "/graphql",
            "DelegatingHandlers": [
                "GraphQlDelegatingHandler"
            ]
        }
    ]
  }
```