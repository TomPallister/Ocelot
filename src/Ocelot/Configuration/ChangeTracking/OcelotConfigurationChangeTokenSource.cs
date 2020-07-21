namespace Ocelot.Configuration.ChangeTracking
{
    using Microsoft.Extensions.Primitives;

    public class OcelotConfigurationChangeTokenSource : IOcelotConfigurationChangeTokenSource
    {
        private readonly OcelotConfigurationChangeToken _changeToken = new OcelotConfigurationChangeToken();

        public IChangeToken ChangeToken => _changeToken;

        public void Activate()
        {
            _changeToken.Activate();
        }
    }
}
