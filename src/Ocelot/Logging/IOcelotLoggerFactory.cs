namespace Ocelot.Logging
{
    public interface IOcelotLoggerFactory
    {
        IOcelotLogger CreateLogger<T>();
    }
}
