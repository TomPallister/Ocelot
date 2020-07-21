using System.Collections.Generic;

namespace Ocelot.Configuration.Creator
{
    public interface IClaimsToThingCreator
    {
        List<ClaimToThing> Create(Dictionary<string, string> thingsBeingAdded);
    }
}
