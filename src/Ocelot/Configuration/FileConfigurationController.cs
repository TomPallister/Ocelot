using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Setter;
using System;
using System.Threading.Tasks;

namespace Ocelot.Configuration
{
    using Repository;

    [Authorize]
    [Route("configuration")]
    public class FileConfigurationController : Controller
    {
        private readonly IFileConfigurationRepository _repo;
        private readonly IFileConfigurationSetter _setter;
        private readonly IServiceProvider _provider;

        public FileConfigurationController(IFileConfigurationRepository repo, IFileConfigurationSetter setter, IServiceProvider provider)
        {
            _repo = repo;
            _setter = setter;
            _provider = provider;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var response = await _repo.Get();

            if (response.IsError)
            {
                return new BadRequestObjectResult(response.Errors);
            }

            return new OkObjectResult(response.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]FileConfiguration fileConfiguration)
        {
            try
            {
                var response = await _setter.Set(fileConfiguration);

                if (response.IsError)
                {
                    return new BadRequestObjectResult(response.Errors);
                }

                return new OkObjectResult(fileConfiguration);
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult($"{e.Message}:{e.StackTrace}");
            }
        }
    }
}
