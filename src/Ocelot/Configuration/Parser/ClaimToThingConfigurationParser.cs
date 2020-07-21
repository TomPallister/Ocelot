using Ocelot.Responses;
using System;
using System.Text.RegularExpressions;

namespace Ocelot.Configuration.Parser
{
    public class ClaimToThingConfigurationParser : IClaimToThingConfigurationParser
    {
        private readonly Regex _claimRegex = new Regex("Claims\\[.*\\]");
        private readonly Regex _indexRegex = new Regex("value\\[.*\\]");
        private const string SplitToken = ">";

        public Response<ClaimToThing> Extract(string existingKey, string value)
        {
            try
            {
                var instructions = value.Split(SplitToken.ToCharArray());

                if (instructions.Length <= 1)
                {
                    return new ErrorResponse<ClaimToThing>(new NoInstructionsError(SplitToken));
                }

                var claimMatch = _claimRegex.IsMatch(instructions[0]);

                if (!claimMatch)
                {
                    return new ErrorResponse<ClaimToThing>(new InstructionNotForClaimsError());
                }

                var newKey = GetIndexValue(instructions[0]);
                var index = 0;
                var delimiter = string.Empty;

                if (instructions.Length > 2 && _indexRegex.IsMatch(instructions[1]))
                {
                    index = int.Parse(GetIndexValue(instructions[1]));
                    delimiter = instructions[2].Trim();
                }

                return new OkResponse<ClaimToThing>(
                               new ClaimToThing(existingKey, newKey, delimiter, index));
            }
            catch (Exception exception)
            {
                return new ErrorResponse<ClaimToThing>(new ParsingConfigurationHeaderError(exception));
            }
        }

        private string GetIndexValue(string instruction)
        {
            var firstIndexer = instruction.IndexOf("[", StringComparison.Ordinal);
            var lastIndexer = instruction.IndexOf("]", StringComparison.Ordinal);
            var length = lastIndexer - firstIndexer;
            var claimKey = instruction.Substring(firstIndexer + 1, length - 1);
            return claimKey;
        }
    }
}
