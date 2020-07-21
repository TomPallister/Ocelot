namespace Ocelot.Multiplexer
{
    using Ocelot.Configuration;

    public interface IResponseAggregatorFactory
    {
        IResponseAggregator Get(Route route);
    }
}
