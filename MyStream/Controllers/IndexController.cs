using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyStream;

namespace WebApp.Controllers
{
   [ApiController]
   [Route("[controller]")]
   public class IndexController : ControllerBase
   {
      private readonly ILogger<IndexController> logger;
      private readonly IWebHostEnvironment env;
      private readonly Database db;

      public IndexController(ILogger<IndexController> logger, IWebHostEnvironment env, Database db)
      {
         this.logger = logger;
         this.env = env;
         this.db = db;
      }

      [HttpGet]
      public async Task<ContentResult> Get()
      {
         var list = new StringBuilder();
         list.Append("<html><body><p>[");
         list.Append(DateTime.Now);
         list.Append("] [Env: ");
         list.Append(env.EnvironmentName);
         list.Append("]</p><h4>Indexes:</h4><ul>");
         var categories = await this.db.GetAllCategories();
         categories.ForEach(category => list.Append(
            $"<li><a href=/stream/byCategoryIndex-{category}/1>{category}</a></li>"));
         list.Append("</ul></body></html>");
         return new ContentResult
         {
            ContentType = "text/html",
            Content = list.ToString()
         };
      }
   }
}
