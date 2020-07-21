namespace Ocelot.RequestId
{
    public class RequestId
    {
        public RequestId(string requestIdKey, string requestIdValue)
        {
            RequestIdKey = requestIdKey;
            RequestIdValue = requestIdValue;
        }

        public string RequestIdKey { get; private set; }
        public string RequestIdValue { get; private set; }
    }
}
