using Ocelot.Errors;

namespace Ocelot.UnitTests.Infrastructure
{
    using Ocelot.Infrastructure.Claims.Parser;
    using Responses;
    using Shouldly;
    using System.Collections.Generic;
    using System.Security.Claims;
    using TestStack.BDDfy;
    using Xunit;

    public class ClaimParserTests
    {
        private readonly IClaimsParser _claimsParser;
        private readonly List<Claim> _claims;
        private string _key;
        private Response<string> _result;
        private string _delimiter;
        private int _index;

        public ClaimParserTests()
        {
            _claims = new List<Claim>();
            _claimsParser = new ClaimsParser();
        }

        [Fact]
        public void can_parse_claims_dictionary_access_string_returning_value_to_function()
        {
            this.Given(x => x.GivenAClaimOf(new Claim("CustomerId", "1234")))
                .And(x => x.GivenTheKeyIs("CustomerId"))
                .When(x => x.WhenICallTheParser())
                .Then(x => x.ThenTheResultIs(new OkResponse<string>("1234")))
                .BDDfy();
        }

        [Fact]
        public void should_return_error_response_when_cannot_find_requested_claim()
        {
            this.Given(x => x.GivenAClaimOf(new Claim("BallsId", "1234")))
                .And(x => x.GivenTheKeyIs("CustomerId"))
                .When(x => x.WhenICallTheParser())
                .Then(x => x.ThenTheResultIs(new ErrorResponse<string>(new List<Error>
                {
                    new CannotFindClaimError($"Cannot find claim for key: {_key}")
                })))
                .BDDfy();
        }

        [Fact]
        public void can_parse_claims_dictionary_access_string_using_delimiter_and_retuning_at_correct_index()
        {
            this.Given(x => x.GivenAClaimOf(new Claim("Subject", "registered|4321")))
                .And(x => x.GivenTheDelimiterIs("|"))
                .And(x => x.GivenTheIndexIs(1))
                .And(x => x.GivenTheKeyIs("Subject"))
                .When(x => x.WhenICallTheParser())
                .Then(x => x.ThenTheResultIs(new OkResponse<string>("4321")))
                .BDDfy();
        }

        [Fact]
        public void should_return_error_response_if_index_too_large()
        {
            this.Given(x => x.GivenAClaimOf(new Claim("Subject", "registered|4321")))
                .And(x => x.GivenTheDelimiterIs("|"))
                .And(x => x.GivenTheIndexIs(24))
                .And(x => x.GivenTheKeyIs("Subject"))
                .When(x => x.WhenICallTheParser())
                .Then(x => x.ThenTheResultIs(new ErrorResponse<string>(new List<Error>
                {
                    new CannotFindClaimError($"Cannot find claim for key: {_key}, delimiter: {_delimiter}, index: {_index}")
                })))
                .BDDfy();
        }

        [Fact]
        public void should_return_error_response_if_index_too_small()
        {
            this.Given(x => x.GivenAClaimOf(new Claim("Subject", "registered|4321")))
                .And(x => x.GivenTheDelimiterIs("|"))
                .And(x => x.GivenTheIndexIs(-1))
                .And(x => x.GivenTheKeyIs("Subject"))
                .When(x => x.WhenICallTheParser())
                .Then(x => x.ThenTheResultIs(new ErrorResponse<string>(new List<Error>
                {
                    new CannotFindClaimError($"Cannot find claim for key: {_key}, delimiter: {_delimiter}, index: {_index}")
                })))
                .BDDfy();
        }

        private void GivenTheIndexIs(int index)
        {
            _index = index;
        }

        private void GivenTheDelimiterIs(string delimiter)
        {
            _delimiter = delimiter;
        }

        private void GivenAClaimOf(Claim claim)
        {
            _claims.Add(claim);
        }

        private void GivenTheKeyIs(string key)
        {
            _key = key;
        }

        private void WhenICallTheParser()
        {
            _result = _claimsParser.GetValue(_claims, _key, _delimiter, _index);
        }

        private void ThenTheResultIs(Response<string> expected)
        {
            _result.Data.ShouldBe(expected.Data);
            _result.IsError.ShouldBe(expected.IsError);
        }
    }
}
