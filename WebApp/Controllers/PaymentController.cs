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

      public PaymentController(ILogger<PaymentController> logger, IPersistenceClient persistence, IMyPayClient myPay)
      {
         this.logger = logger;
         this.app = new Payment(persistence, myPay);
      }

      [HttpPost]
      public async Task<ActionResult> Post([FromBody] JsonElement cmd)
      {
         try
         {
            await this.app.Pay(
               cmd.GetProperty("orderId").GetString(),
               cmd.GetProperty("correlationId").GetString());
         }
         catch (Exception e)
         {
            this.logger.LogError(e.ToString());
            return BadRequest();
         }
         return Ok();
      }

      [HttpPost("finalize")]
      public async Task<ActionResult> Finalize([FromBody] JsonElement cmd)
      {
         try
         {
            var isFinalized = await this.app.FinalizePayment(
               cmd.GetProperty("paymentId").GetString(),
               cmd.GetProperty("correlationId").GetString(),
               cmd.GetProperty("total").GetDecimal(),
               cmd.GetProperty("description").GetString());

            return isFinalized ? Ok() : Accepted() as ActionResult;
         }
         catch (Exception e)
         {
            this.logger.LogError($"Finalize fail: {e.ToString()}");
            return BadRequest();
         }
      }
   }
}
