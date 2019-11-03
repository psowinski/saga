using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using App;
using Persistence;
using System.Text.Json;
using Common.General;

namespace WebApp.Controllers
{
   [ApiController]
   [Route("[controller]")]
   public class OrderController : ControllerBase
   {
      private readonly ILogger<OrderController> logger;
      private readonly Order app;

      public OrderController(ILogger<OrderController> logger, IPersistenceClient persistence)
      {
         this.logger = logger;
         this.app = new Order(persistence);
      }

      [HttpPost("additem")]
      public async Task<ActionResult> AddItem([FromBody] JsonElement cmd)
      {
         try
         {
            await this.app.AddItem(
               new Some<string>(cmd.GetProperty("orderId").GetString()),
               cmd.GetProperty("correlationId").GetString(),
               cmd.GetProperty("description").GetString(),
               cmd.GetProperty("cost").GetDecimal());
         }
         catch (Exception e)
         {
            this.logger.LogError(e.Message);
            return BadRequest();
         }
         return Ok();
      }

      [HttpPost("checkout")]
      public async Task<ActionResult> Checkout([FromBody] JsonElement cmd)
      {
         try
         {
            await this.app.Checkout(
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
