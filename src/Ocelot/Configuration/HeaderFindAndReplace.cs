namespace Ocelot.Configuration
{
    public class HeaderFindAndReplace
    {
        public HeaderFindAndReplace(string key, string find, string replace, int index)
        {
            Key = key;
            Find = find;
            Replace = replace;
            Index = index;
        }

        public string Key { get; }
        public string Find { get; }
        public string Replace { get; }

        // only index 0 for now..
        public int Index { get; }
    }
}
