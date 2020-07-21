```json
{
  "routes": [
    {
      "downstreamPathTemplate": "/{everything}",
      "upstreamPathTemplate": "/templates/{everything}",
      "upstreamHttpMethod": [
        "GET"
      ],
      "addHeadersToRequest": {},
      "upstreamHeaderTransform": {},
      "downstreamHeaderTransform": {},
      "addClaimsToRequest": {},
      "routeClaimsRequirement": {},
      "addQueriesToRequest": {},
      "requestIdKey": null,
      "fileCacheOptions": {
        "ttlSeconds": 0,
        "region": null
      },
      "routeIsCaseSensitive": false,
      "downstreamScheme": "http",
      "qoSOptions": {
        "exceptionsAllowedBeforeBreaking": 0,
        "durationOfBreak": 0,
        "timeoutValue": 0
      },
      "loadBalancerOptions": {
        "type": null,
        "key": null,
        "expiry": 0
      },
      "rateLimitOptions": {
        "clientWhitelist": [],
        "enableRateLimiting": false,
        "period": null,
        "periodTimespan": 0,
        "limit": 0
      },
      "authenticationOptions": {
        "authenticationProviderKey": null,
        "allowedScopes": []
      },
      "httpHandlerOptions": {
        "allowAutoRedirect": false,
        "useCookieContainer": false,
        "useTracing": false,
        "useProxy": true
      },
      "downstreamHostAndPorts": [
        {
          "host": "localhost",
          "port": 50689
        }
      ],
      "upstreamHost": null,
      "key": null,
      "delegatingHandlers": [],
      "priority": 1,
      "timeout": 0,
      "dangerousAcceptAnyServerCertificateValidator": false
    }
  ],
  "aggregates": [],
  "globalConfiguration": {
    "requestIdKey": "Request-Id",
    "rateLimitOptions": {
      "clientIdHeader": "ClientId",
      "quotaExceededMessage": null,
      "rateLimitCounterPrefix": "ocelot",
      "disableRateLimitHeaders": false,
      "httpStatusCode": 429
    },
    "qoSOptions": {
      "exceptionsAllowedBeforeBreaking": 0,
      "durationOfBreak": 0,
      "timeoutValue": 0
    },
    "baseUrl": "http://localhost:55580",
    "loadBalancerOptions": {
      "type": null,
      "key": null,
      "expiry": 0
    },
    "downstreamScheme": null,
    "httpHandlerOptions": {
      "allowAutoRedirect": false,
      "useCookieContainer": false,
      "useTracing": false,
      "useProxy": true
    }
  }
}
```
