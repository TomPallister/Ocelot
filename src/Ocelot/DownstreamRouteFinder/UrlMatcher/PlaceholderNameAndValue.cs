namespace Ocelot.DownstreamRouteFinder.UrlMatcher
{
    public class PlaceholderNameAndValue
    {
        public PlaceholderNameAndValue(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; private set; }
        public string Value { get; private set; }
    }
}
