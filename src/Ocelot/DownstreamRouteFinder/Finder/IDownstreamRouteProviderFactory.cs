namespace Ocelot.DownstreamRouteFinder.Finder
{
    using Configuration;

    public interface IDownstreamRouteProviderFactory
    {
        IDownstreamRouteProvider Get(IInternalConfiguration config);
    }
}
