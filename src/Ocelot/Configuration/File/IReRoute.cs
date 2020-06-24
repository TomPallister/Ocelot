namespace Ocelot.Configuration.File
{
    public interface IRoute
    {
        string UpstreamPathTemplate { get; set; }
        bool RouteIsCaseSensitive { get; set; }
        int Priority { get; set; }
    }
}
