namespace Ocelot.Configuration.Creator
{
    using System;

    public interface IVersionCreator
    {
        Version Create(string downstreamHttpVersion);
    }
}
