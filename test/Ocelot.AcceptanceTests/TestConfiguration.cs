using System;
using System.IO;

namespace Ocelot.AcceptanceTests
{
    public static class TestConfiguration
    {
        public static string ConfigurationPath => Path.Combine(AppContext.BaseDirectory, "ocelot.json");
    }
}
