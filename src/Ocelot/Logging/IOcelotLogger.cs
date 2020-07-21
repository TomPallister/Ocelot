using System;

namespace Ocelot.Logging
{
    /// <summary>
    /// Thin wrapper around the DotNet core logging framework, used to allow the scopedDataRepository to be injected giving access to the Ocelot RequestId
    /// </summary>
    public interface IOcelotLogger
    {
        void LogTrace(string message);

        void LogDebug(string message);

        void LogInformation(string message);

        void LogWarning(string message);

        void LogError(string message, Exception exception);

        void LogCritical(string message, Exception exception);
    }
}
