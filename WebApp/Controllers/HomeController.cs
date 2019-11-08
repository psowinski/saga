using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApp.Controllers
{
   [ApiController]
   [Route("[controller]")]
   public class HomeController : ControllerBase
   {
      private readonly ILogger<HomeController> logger;
      private readonly IWebHostEnvironment env;

      public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env)
      {
         this.logger = logger;
         this.env = env;
      }

      [HttpGet]
      public ActionResult<string> Get()
      {
         return $"Happy & running [{DateTime.Now}] [env: {env.EnvironmentName}]";
      }
   }
}
