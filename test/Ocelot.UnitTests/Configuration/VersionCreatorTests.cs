namespace Ocelot.UnitTests.Configuration
{
    using System;
    using Ocelot.Configuration.Creator;
    using Shouldly;
    using TestStack.BDDfy;
    using Xunit;

    public class VersionCreatorTests
    {
        private readonly HttpVersionCreator _creator;
        private string _input;
        private Version _result;

        public VersionCreatorTests()
        {
            _creator = new HttpVersionCreator();
        }

        [Fact]
        public void should_create_version_based_on_input()
        {
            this.Given(_ => GivenTheInput("2.0"))
                .When(_ => WhenICreate())
                .Then(_ => ThenTheResultIs(2, 0))
                .BDDfy();
        }

        [Fact]
        public void should_default_to_version_one_point_one()
        {
            this.Given(_ => GivenTheInput(""))
                .When(_ => WhenICreate())
                .Then(_ => ThenTheResultIs(1, 1))
                .BDDfy();
        }

        private void GivenTheInput(string input)
        {
            _input = input;
        }

        private void WhenICreate()
        {
            _result = _creator.Create(_input);
        }

        private void ThenTheResultIs(int major, int minor)
        {
            _result.Major.ShouldBe(major);
            _result.Minor.ShouldBe(minor);
        }
    }
}
