using Ocelot.Configuration.Parser;
using Ocelot.Logging;
using System.Collections.Generic;

namespace Ocelot.Configuration.Creator
{
    public class ClaimsToThingCreator : IClaimsToThingCreator
    {
        private readonly IClaimToThingConfigurationParser _claimToThingConfigParser;
        private readonly IOcelotLogger _logger;

        public ClaimsToThingCreator(IClaimToThingConfigurationParser claimToThingConfigurationParser,
            IOcelotLoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ClaimsToThingCreator>();
            _claimToThingConfigParser = claimToThingConfigurationParser;
        }

        public List<ClaimToThing> Create(Dictionary<string, string> inputToBeParsed)
        {
            var claimsToThings = new List<ClaimToThing>();

            foreach (var input in inputToBeParsed)
            {
                var claimToThing = _claimToThingConfigParser.Extract(input.Key, input.Value);

                if (claimToThing.IsError)
                {
                    _logger.LogDebug($"Unable to extract configuration for key: {input.Key} and value: {input.Value} your configuration file is incorrect");
                }
                else
                {
                    claimsToThings.Add(claimToThing.Data);
                }
            }

            return claimsToThings;
        }
    }
}
