namespace Ocelot.Middleware
{
    using Ocelot.Configuration;
    using Ocelot.DownstreamRouteFinder;
    using Ocelot.DownstreamRouteFinder.UrlMatcher;
    using Ocelot.Errors;
    using Ocelot.Request.Middleware;
    using System.Collections.Generic;

    public static class HttpItemsExtensions
    {
        public static void UpsertDownstreamRequest(this IDictionary<object, object> input, DownstreamRequest downstreamRequest)
        {
            input.Upsert("DownstreamRequest", downstreamRequest);
        }

        public static void UpsertDownstreamResponse(this IDictionary<object, object> input, DownstreamResponse downstreamResponse)
        {
            input.Upsert("DownstreamResponse", downstreamResponse);
        }

        public static void UpsertDownstreamRoute(this IDictionary<object, object> input, Configuration.DownstreamRoute downstreamRoute)
        {
            input.Upsert("DownstreamRoute", downstreamRoute);
        }

        public static void UpsertTemplatePlaceholderNameAndValues(this IDictionary<object, object> input, List<PlaceholderNameAndValue> tPNV)
        {
            input.Upsert("TemplatePlaceholderNameAndValues", tPNV);
        }

        public static void UpsertDownstreamRoute(this IDictionary<object, object> input, DownstreamRouteFinder.DownstreamRouteHolder downstreamRoute)
        {
            input.Upsert("DownstreamRouteHolder", downstreamRoute);
        }

        public static void UpsertErrors(this IDictionary<object, object> input, List<Error> errors)
        {
            input.Upsert("Errors", errors);
        }

        public static void SetError(this IDictionary<object, object> input, Error error)
        {
            var errors = new List<Error>() { error };
            input.Upsert("Errors", errors);
        }

        public static void SetIInternalConfiguration(this IDictionary<object, object> input, IInternalConfiguration config)
        {
            input.Upsert("IInternalConfiguration", config);
        }

        public static IInternalConfiguration IInternalConfiguration(this IDictionary<object, object> input)
        {
            return input.Get<IInternalConfiguration>("IInternalConfiguration");
        }

        public static List<Error> Errors(this IDictionary<object, object> input)
        {
            var errors = input.Get<List<Error>>("Errors");
            return errors == null ? new List<Error>() : errors;
        }

        public static DownstreamRouteFinder.DownstreamRouteHolder DownstreamRouteHolder(this IDictionary<object, object> input)
        {
            return input.Get<DownstreamRouteFinder.DownstreamRouteHolder>("DownstreamRouteHolder");
        }

        public static List<PlaceholderNameAndValue> TemplatePlaceholderNameAndValues(this IDictionary<object, object> input)
        {
            return input.Get<List<PlaceholderNameAndValue>>("TemplatePlaceholderNameAndValues");
        }

        public static DownstreamRequest DownstreamRequest(this IDictionary<object, object> input)
        {
            return input.Get<DownstreamRequest>("DownstreamRequest");
        }

        public static DownstreamResponse DownstreamResponse(this IDictionary<object, object> input)
        {
            return input.Get<DownstreamResponse>("DownstreamResponse");
        }

        public static Configuration.DownstreamRoute DownstreamRoute(this IDictionary<object, object> input)
        {
            return input.Get<Configuration.DownstreamRoute>("DownstreamRoute");
        }

        private static T Get<T>(this IDictionary<object, object> input, string key)
        {
            if (input.TryGetValue(key, out var value))
            {
                return (T)value;
            }

            return default(T);
        }

        private static void Upsert<T>(this IDictionary<object, object> input, string key, T value)
        {
            if (input.DoesntExist(key))
            {
                input.Add(key, value);
            }
            else
            {
                input.Remove(key);
                input.Add(key, value);
            }
        }

        private static bool DoesntExist(this IDictionary<object, object> input, string key)
        {
            return !input.ContainsKey(key);
        }
    }
}
