namespace Ocelot.UnitTests.Configuration
{
    using Ocelot.Configuration;
    using Ocelot.Configuration.Creator;
    using Ocelot.Configuration.File;
    using Shouldly;
    using TestStack.BDDfy;
    using Xunit;

    public class LoadBalancerOptionsCreatorTests
    {
        private readonly ILoadBalancerOptionsCreator _creator;
        private FileLoadBalancerOptions _fileLoadBalancerOptions;
        private LoadBalancerOptions _result;

        public LoadBalancerOptionsCreatorTests()
        {
            _creator = new LoadBalancerOptionsCreator();
        }

        [Fact]
        public void should_create()
        {
            var fileLoadBalancerOptions = new FileLoadBalancerOptions
            {
                Type = "test",
                Key = "west",
                Expiry = 1
            };

            this.Given(_ => GivenThe(fileLoadBalancerOptions))
                .When(_ => WhenICreate())
                .Then(_ => ThenTheOptionsAreCreated(fileLoadBalancerOptions))
                .BDDfy();
        }

        private void ThenTheOptionsAreCreated(FileLoadBalancerOptions expected)
        {
            _result.Type.ShouldBe(expected.Type);
            _result.Key.ShouldBe(expected.Key);
            _result.ExpiryInMs.ShouldBe(expected.Expiry);
        }

        private void WhenICreate()
        {
            _result = _creator.Create(_fileLoadBalancerOptions);
        }

        private void GivenThe(FileLoadBalancerOptions fileLoadBalancerOptions)
        {
            _fileLoadBalancerOptions = fileLoadBalancerOptions;
        }
    }
}
