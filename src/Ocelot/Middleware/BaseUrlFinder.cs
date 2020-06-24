using Microsoft.Extensions.Configuration;

namespace Ocelot.Middleware
{
    public class BaseUrlFinder : IBaseUrlFinder
    {
        private readonly IConfiguration _config;

        public BaseUrlFinder(IConfiguration config)
        {
            _config = config;
        }

        public string Find()
        {
            //tries to get base url out of file...
            var baseUrl = _config.GetValue("GlobalConfiguration:BaseUrl", "");

            //falls back to memory config then finally default..
            return string.IsNullOrEmpty(baseUrl) ? _config.GetValue("BaseUrl", "http://localhost:5000") : baseUrl;
        }
    }
}
