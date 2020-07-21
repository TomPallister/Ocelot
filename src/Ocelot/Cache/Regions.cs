namespace Ocelot.Cache
{
    using System.Collections.Generic;

    public class Regions
    {
        public Regions(List<string> value)
        {
            Value = value;
        }

        public List<string> Value { get; }
    }
}
