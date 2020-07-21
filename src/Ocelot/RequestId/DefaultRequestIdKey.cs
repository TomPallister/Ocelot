namespace Ocelot.RequestId
{
    public static class DefaultRequestIdKey
    {
        // This is set incase anyone isnt doing this specifically with there requests.
        // It will not be forwarded on to downstream services unless specfied in the config.
        public const string Value = "RequestId";
    }
}
