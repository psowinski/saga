using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using App;
using Persistence;

namespace WebApp.Controllers
{
   [ApiController]
   [Route("[controller]")]
   public class PaymentController : ControllerBase
   {
      private readonly ILogger<PaymentController> logger;
      private readonly Payment app;

      public PaymentController(ILogger<PaymentController> logger, IPersistenceClient persistence)
      {
         this.logger = logger;
         this.app = new Payment(persistence);
      }

      [HttpPost]
      public async Task<ActionResult> Post([FromBody] JsonElement cmd)
      {
         try
         {
            await this.app.Pay_v1(
               cmd.GetProperty("orderId").GetString(),
               cmd.GetProperty("correlationId").GetString());
         }
         catch (Exception e)
         {
            this.logger.LogError(e.Message);
            return BadRequest();
         }
         return Ok();
      }
   }
}
