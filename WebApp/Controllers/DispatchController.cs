using System;
using System.Text.Json;
using System.Threading.Tasks;
using App;
using Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApp.Controllers
{
   [ApiController]
   [Route("[controller]")]
   public class DispatchController : ControllerBase
   {
      private readonly ILogger<DispatchController> logger;
      private readonly Dispatch app;
      public DispatchController(ILogger<DispatchController> logger, IPersistenceClient persistence)
      {
         this.logger = logger;
         this.app = new Dispatch(persistence);
      }

      [HttpPost]
      public async Task<ActionResult> Post([FromBody] JsonElement cmd)
      {
         try
         {
            await this.app.DispatchOrder(
               cmd.GetProperty("orderId").GetString(), 
               cmd.GetProperty("paymentId").GetString(),
               cmd.GetProperty("correlationId").GetString());
         }
         catch (Exception e)
         {
            this.logger.LogError(e.ToString());
            return BadRequest();
         }
         return Ok();
      }
   }
}
