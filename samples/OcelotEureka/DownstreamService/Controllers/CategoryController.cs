using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace DownstreamService.Controllers
{
    [Route("api/[controller]")]
    public class CategoryController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new[] { "category1", "category2" };
        }
    }
}
