namespace Ocelot.Configuration.Creator
{
    using Ocelot.Configuration.Builder;
    using Ocelot.Configuration.File;
    using System.Collections.Generic;
    using System.Linq;

    public class QoSOptionsCreator : IQoSOptionsCreator
    {
        public QoSOptions Create(FileQoSOptions options)
        {
            return new QoSOptionsBuilder()
                .WithExceptionsAllowedBeforeBreaking(options.ExceptionsAllowedBeforeBreaking)
                .WithDurationOfBreak(options.DurationOfBreak)
                .WithTimeoutValue(options.TimeoutValue)
                .Build();
        }

        public QoSOptions Create(FileQoSOptions options, string pathTemplate, List<string> httpMethods)
        {
            var key = CreateKey(pathTemplate, httpMethods);

            return Map(key, options.TimeoutValue, options.DurationOfBreak, options.ExceptionsAllowedBeforeBreaking);
        }

        public QoSOptions Create(QoSOptions options, string pathTemplate, List<string> httpMethods)
        {
            var key = CreateKey(pathTemplate, httpMethods);

            return Map(key, options.TimeoutValue, options.DurationOfBreak, options.ExceptionsAllowedBeforeBreaking);
        }

        private QoSOptions Map(string key, int timeoutValue, int durationOfBreak, int exceptionsAllowedBeforeBreaking)
        {
            return new QoSOptionsBuilder()
                .WithExceptionsAllowedBeforeBreaking(exceptionsAllowedBeforeBreaking)
                .WithDurationOfBreak(durationOfBreak)
                .WithTimeoutValue(timeoutValue)
                .WithKey(key)
                .Build();
        }

        private string CreateKey(string pathTemplate, List<string> httpMethods)
        {
            return $"{pathTemplate.FirstOrDefault()}|{string.Join(",", httpMethods)}";
        }
    }
}
