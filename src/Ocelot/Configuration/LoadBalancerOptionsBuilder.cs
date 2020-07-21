namespace Ocelot.Configuration
{
    public class LoadBalancerOptionsBuilder
    {
        private string _type;
        private string _key;
        private int _expiryInMs;

        public LoadBalancerOptionsBuilder WithType(string type)
        {
            _type = type;
            return this;
        }

        public LoadBalancerOptionsBuilder WithKey(string key)
        {
            _key = key;
            return this;
        }

        public LoadBalancerOptionsBuilder WithExpiryInMs(int expiryInMs)
        {
            _expiryInMs = expiryInMs;
            return this;
        }

        public LoadBalancerOptions Build()
        {
            return new LoadBalancerOptions(_type, _key, _expiryInMs);
        }
    }
}
