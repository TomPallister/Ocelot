namespace Ocelot.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;

    public class OcelotPipelineConfiguration
    {
        /// <summary>
        /// This is called after the global error handling middleware so any code before calling next.invoke
        /// is the next thing called in the Ocelot pipeline. Anything after next.invoke is the last thing called
        /// in the Ocelot pipeline before we go to the global error handler.
        /// </summary>
        /// <value>
        /// <placeholder>This is called after the global error handling middleware so any code before calling next.invoke
        /// is the next thing called in the Ocelot pipeline. Anything after next.invoke is the last thing called
        /// in the Ocelot pipeline before we go to the global error handler.</placeholder>
        /// </value>
        public Func<HttpContext, Func<Task>, Task> PreErrorResponderMiddleware { get; set; }

        /// <summary>
        /// This is to allow the user to run any extra authentication before the Ocelot authentication
        /// kicks in
        /// </summary>
        /// <value>
        /// <placeholder>This is to allow the user to run any extra authentication before the Ocelot authentication
        /// kicks in</placeholder>
        /// </value>
        public Func<HttpContext, Func<Task>, Task> PreAuthenticationMiddleware { get; set; }

        /// <summary>
        /// This allows the user to completely override the ocelot authentication middleware
        /// </summary>
        /// <value>
        /// <placeholder>This allows the user to completely override the ocelot authentication middleware</placeholder>
        /// </value>
        public Func<HttpContext, Func<Task>, Task> AuthenticationMiddleware { get; set; }

        /// <summary>
        /// This is to allow the user to run any extra authorisation before the Ocelot authentication
        /// kicks in
        /// </summary>
        /// <value>
        /// <placeholder>This is to allow the user to run any extra authorisation before the Ocelot authentication
        /// kicks in</placeholder>
        /// </value>
        public Func<HttpContext, Func<Task>, Task> PreAuthorisationMiddleware { get; set; }

        /// <summary>
        /// This allows the user to completely override the ocelot authorisation middleware
        /// </summary>
        /// <value>
        /// <placeholder>This allows the user to completely override the ocelot authorisation middleware</placeholder>
        /// </value>
        public Func<HttpContext, Func<Task>, Task> AuthorisationMiddleware { get; set; }

        /// <summary>
        /// This allows the user to implement there own query string manipulation logic
        /// </summary>
        /// <value>
        /// <placeholder>This allows the user to implement there own query string manipulation logic</placeholder>
        /// </value>
        public Func<HttpContext, Func<Task>, Task> PreQueryStringBuilderMiddleware { get; set; }

        /// <summary>
        /// This is an extension that will branch to different pipes
        /// </summary>
        /// <value>
        /// <placeholder>This is an extension that will branch to different pipes</placeholder>
        /// </value>
        // todo fix this data structure
        public Dictionary<Func<HttpContext,  bool>, Action<IApplicationBuilder>> MapWhenOcelotPipeline { get; } = new Dictionary<Func<HttpContext, bool>, Action<IApplicationBuilder>>();
    }
}
