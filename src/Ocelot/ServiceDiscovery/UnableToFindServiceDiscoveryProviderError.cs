namespace Ocelot.ServiceDiscovery
{
    using Ocelot.Errors;

    public class UnableToFindServiceDiscoveryProviderError : Error
    {
        public UnableToFindServiceDiscoveryProviderError(string message) : base(message, OcelotErrorCode.UnableToFindServiceDiscoveryProviderError, 404)
        {
        }
    }
}
