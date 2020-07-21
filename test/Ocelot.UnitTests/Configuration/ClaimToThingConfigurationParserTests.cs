using Ocelot.Configuration;
using Ocelot.Configuration.Parser;
using Ocelot.Errors;
using Ocelot.Responses;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Configuration
{
    public class ClaimToThingConfigurationParserTests
    {
        private Dictionary<string, string> _dictionary;
        private readonly IClaimToThingConfigurationParser _claimToThingConfigurationParser;
        private Response<ClaimToThing> _result;

        public ClaimToThingConfigurationParserTests()
        {
            _claimToThingConfigurationParser = new ClaimToThingConfigurationParser();
        }

        [Fact]
        public void returns_no_instructions_error()
        {
            this.Given(x => x.GivenTheDictionaryIs(new Dictionary<string, string>()
            {
                {"CustomerId", ""},
            }))
                .When(x => x.WhenICallTheExtractor())
                .Then(
                    x =>
                        x.ThenAnErrorIsReturned(new ErrorResponse<ClaimToThing>(
                            new List<Error>
                            {
                                new NoInstructionsError(">")
                            })))
                .BDDfy();
        }

        [Fact]
        public void returns_no_instructions_not_for_claims_error()
        {
            this.Given(x => x.GivenTheDictionaryIs(new Dictionary<string, string>()
            {
                {"CustomerId", "Cheese[CustomerId] > value"},
            }))
                .When(x => x.WhenICallTheExtractor())
                .Then(
                    x =>
                        x.ThenAnErrorIsReturned(new ErrorResponse<ClaimToThing>(
                            new List<Error>
                            {
                                new InstructionNotForClaimsError()
                            })))
                .BDDfy();
        }

        [Fact]
        public void can_parse_entry_to_work_out_properties_with_key()
        {
            this.Given(x => x.GivenTheDictionaryIs(new Dictionary<string, string>()
            {
                {"CustomerId", "Claims[CustomerId] > value"},
            }))
                .When(x => x.WhenICallTheExtractor())
                .Then(
                    x =>
                        x.ThenTheClaimParserPropertiesAreReturned(
                            new OkResponse<ClaimToThing>(
                                new ClaimToThing("CustomerId", "CustomerId", "", 0))))
                .BDDfy();
        }

        [Fact]
        public void can_parse_entry_to_work_out_properties_with_key_delimiter_and_index()
        {
            this.Given(x => x.GivenTheDictionaryIs(new Dictionary<string, string>()
            {
                {"UserId", "Claims[Subject] > value[0] > |"},
            }))
                .When(x => x.WhenICallTheExtractor())
                .Then(
                    x =>
                        x.ThenTheClaimParserPropertiesAreReturned(
                            new OkResponse<ClaimToThing>(
                                new ClaimToThing("UserId", "Subject", "|", 0))))
                .BDDfy();
        }

        private void ThenAnErrorIsReturned(Response<ClaimToThing> expected)
        {
            _result.IsError.ShouldBe(expected.IsError);
            _result.Errors[0].ShouldBeOfType(expected.Errors[0].GetType());
        }

        private void ThenTheClaimParserPropertiesAreReturned(Response<ClaimToThing> expected)
        {
            _result.Data.NewKey.ShouldBe(expected.Data.NewKey);
            _result.Data.Delimiter.ShouldBe(expected.Data.Delimiter);
            _result.Data.Index.ShouldBe(expected.Data.Index);
            _result.IsError.ShouldBe(expected.IsError);
        }

        private void WhenICallTheExtractor()
        {
            var first = _dictionary.First();
            _result = _claimToThingConfigurationParser.Extract(first.Key, first.Value);
        }

        private void GivenTheDictionaryIs(Dictionary<string, string> dictionary)
        {
            _dictionary = dictionary;
        }
    }
}
