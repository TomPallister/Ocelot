using System;
using System.IO;

namespace Ocelot.AcceptanceTests
{
    public static class TestConfiguration
    {
        public static string PrimaryConfigurationPath => Path.Combine(AppContext.BaseDirectory, "ocelot.json");

        public static string ConfigurationPartPath(string configurationPart) => Path.Combine(AppContext.BaseDirectory, $"ocelot.{configurationPart}.json");
    }
}
