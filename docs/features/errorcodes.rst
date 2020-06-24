Http Error Status Codes
=======================

Ocelot will return HTTP status error codes based on internal logic in certain siturations:
- 401 if the authentication middleware runs and the user is not authenticated.
- 403 if the authorisation middleware runs and the user is unauthenticated, claim value not authroised, scope not authorised, user doesnt have required claim or cannot find claim.
- 503 if the downstream request times out.
- 499 if the request is cancelled by the client.
- 404 if unable to find a downstream route.
- 502 if unable to connect to downstream service.
- 500 if unable to complete the HTTP request downstream and the exception is not OperationCanceledException or HttpRequestException.
- 404 if Ocelot is unable to map an internal error code to a HTTP status code.

