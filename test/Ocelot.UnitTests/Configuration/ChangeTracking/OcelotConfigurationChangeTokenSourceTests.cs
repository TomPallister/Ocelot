namespace Ocelot.UnitTests.Configuration.ChangeTracking
{
    using Ocelot.Configuration.ChangeTracking;
    using Shouldly;
    using TestStack.BDDfy;
    using Xunit;

    public class OcelotConfigurationChangeTokenSourceTests
    {
        private readonly IOcelotConfigurationChangeTokenSource _source;

        public OcelotConfigurationChangeTokenSourceTests()
        {
            _source = new OcelotConfigurationChangeTokenSource();
        }

        [Fact]
        public void should_activate_change_token()
        {
            this.Given(_ => GivenIActivateTheChangeTokenSource())
                .Then(_ => ThenTheChangeTokenShouldBeActivated())
                .BDDfy();
        }

        private void GivenIActivateTheChangeTokenSource()
        {
            _source.Activate();
        }

        private void ThenTheChangeTokenShouldBeActivated()
        {
            _source.ChangeToken.HasChanged.ShouldBeTrue();
        }
    }
}
