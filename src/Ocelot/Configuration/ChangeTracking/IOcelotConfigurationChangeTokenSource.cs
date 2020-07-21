namespace Ocelot.Configuration.ChangeTracking
{
    using Microsoft.Extensions.Primitives;

    /// <summary>
    /// <see cref="IChangeToken" /> source which is activated when Ocelot's configuration is changed.
    /// </summary>
    public interface IOcelotConfigurationChangeTokenSource
    {
        IChangeToken ChangeToken { get; }

        void Activate();
    }
}
