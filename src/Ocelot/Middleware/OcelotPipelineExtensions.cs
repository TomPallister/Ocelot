namespace Ocelot.Middleware
{
    using Ocelot.QueryStrings.Middleware;
    using Ocelot.RateLimit.Middleware;
    using Ocelot.Request.Middleware;
    using Ocelot.Requester.Middleware;
    using Ocelot.RequestId.Middleware;
    using Ocelot.Responder.Middleware;
    using Ocelot.Security.Middleware;
    using Ocelot.Authentication.Middleware;
    using Ocelot.Authorisation.Middleware;
    using Ocelot.Cache.Middleware;
    using Ocelot.Claims.Middleware;
    using Ocelot.DownstreamRouteFinder.Middleware;
    using Ocelot.DownstreamUrlCreator.Middleware;
    using Ocelot.Errors.Middleware;
    using Ocelot.Headers.Middleware;
    using Ocelot.LoadBalancer.Middleware;
    using System;
    using System.Threading.Tasks;
    using Ocelot.DownstreamPathManipulation.Middleware;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Ocelot.WebSockets.Middleware;
    using Ocelot.Multiplexer;

    public static class OcelotPipelineExtensions
    {
        public static RequestDelegate BuildOcelotPipeline(this IApplicationBuilder app,
            OcelotPipelineConfiguration pipelineConfiguration)
        {
            // this sets up the downstream context and gets the config
            app.UseDownstreamContextMiddleware();

            // This is registered to catch any global exceptions that are not handled
            // It also sets the Request Id if anything is set globally
            app.UseExceptionHandlerMiddleware();

            // If the request is for websockets upgrade we fork into a different pipeline
            app.MapWhen(httpContext => httpContext.WebSockets.IsWebSocketRequest,
                wenSocketsApp =>
                {
                    wenSocketsApp.UseDownstreamRouteFinderMiddleware();
                    wenSocketsApp.UseMultiplexingMiddleware();
                    wenSocketsApp.UseDownstreamRequestInitialiser();
                    wenSocketsApp.UseLoadBalancingMiddleware();
                    wenSocketsApp.UseDownstreamUrlCreatorMiddleware();
                    wenSocketsApp.UseWebSocketsProxyMiddleware();
                });

            // Allow the user to respond with absolutely anything they want.
            app.UseIfNotNull(pipelineConfiguration.PreErrorResponderMiddleware);

            // This is registered first so it can catch any errors and issue an appropriate response
            app.UseResponderMiddleware();

            // Then we get the downstream route information
            app.UseDownstreamRouteFinderMiddleware();

            // Multiplex the request if required
            app.UseMultiplexingMiddleware();

            // This security module, IP whitelist blacklist, extended security mechanism
            app.UseSecurityMiddleware();

            //Expand other branch pipes
            if (pipelineConfiguration.MapWhenOcelotPipeline != null)
            {
                foreach (var pipeline in pipelineConfiguration.MapWhenOcelotPipeline)
                {
                    // todo why is this asking for an app app?
                    app.MapWhen(pipeline.Key, pipeline.Value);
                }
            }

            // Now we have the ds route we can transform headers and stuff?
            app.UseHttpHeadersTransformationMiddleware();

            // Initialises downstream request
            app.UseDownstreamRequestInitialiser();

            // We check whether the request is ratelimit, and if there is no continue processing
            app.UseRateLimiting();

            // This adds or updates the request id (initally we try and set this based on global config in the error handling middleware)
            // If anything was set at global level and we have a different setting at re route level the global stuff will be overwritten
            // This means you can get a scenario where you have a different request id from the first piece of middleware to the request id middleware.
            app.UseRequestIdMiddleware();

            // Allow pre authentication logic. The idea being people might want to run something custom before what is built in.
            app.UseIfNotNull(pipelineConfiguration.PreAuthenticationMiddleware);

            // Now we know where the client is going to go we can authenticate them.
            // We allow the ocelot middleware to be overriden by whatever the
            // user wants
            if (pipelineConfiguration.AuthenticationMiddleware == null)
            {
                app.UseAuthenticationMiddleware();
            }
            else
            {
                app.Use(pipelineConfiguration.AuthenticationMiddleware);
            }

            // The next thing we do is look at any claims transforms in case this is important for authorisation
            app.UseClaimsToClaimsMiddleware();

            // Allow pre authorisation logic. The idea being people might want to run something custom before what is built in.
            app.UseIfNotNull(pipelineConfiguration.PreAuthorisationMiddleware);

            // Now we have authenticated and done any claims transformation we
            // can authorise the request
            // We allow the ocelot middleware to be overriden by whatever the
            // user wants
            if (pipelineConfiguration.AuthorisationMiddleware == null)
            {
                app.UseAuthorisationMiddleware();
            }
            else
            {
                app.Use(pipelineConfiguration.AuthorisationMiddleware);
            }

            // Now we can run the claims to headers transformation middleware
            app.UseClaimsToHeadersMiddleware();

            // Allow the user to implement their own query string manipulation logic
            app.UseIfNotNull(pipelineConfiguration.PreQueryStringBuilderMiddleware);

            // Now we can run any claims to query string transformation middleware
            app.UseClaimsToQueryStringMiddleware();

            app.UseClaimsToDownstreamPathMiddleware();

            // Get the load balancer for this request
            app.UseLoadBalancingMiddleware();

            // This takes the downstream route we retrieved earlier and replaces any placeholders with the variables that should be used
            app.UseDownstreamUrlCreatorMiddleware();

            // Not sure if this is the best place for this but we use the downstream url
            // as the basis for our cache key.
            app.UseOutputCacheMiddleware();

            //We fire off the request and set the response on the scoped data repo
            app.UseHttpRequesterMiddleware();

            return app.Build();
        }

        private static void UseIfNotNull(this IApplicationBuilder builder,
            Func<HttpContext, Func<Task>, Task> middleware)
        {
            if (middleware != null)
            {
                builder.Use(middleware);
            }
        }
    }
}
