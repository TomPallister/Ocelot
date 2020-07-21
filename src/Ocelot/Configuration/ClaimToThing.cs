namespace Ocelot.Configuration
{
    public class ClaimToThing
    {
        public ClaimToThing(string existingKey, string newKey, string delimiter, int index)
        {
            NewKey = newKey;
            Delimiter = delimiter;
            Index = index;
            ExistingKey = existingKey;
        }

        public string ExistingKey { get; private set; }
        public string NewKey { get; private set; }
        public string Delimiter { get; private set; }
        public int Index { get; private set; }
    }
}
