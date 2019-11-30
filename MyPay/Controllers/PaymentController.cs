using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MyPay.Controllers
{
   [ApiController]
   [Route("[controller]")]
   public class PaymentController : ControllerBase
   {
      private readonly ILogger<PaymentController> logger;
      private readonly MyPayService myPayService;

      public PaymentController(ILogger<PaymentController> logger, MyPayService myPayService)
      {
         this.logger = logger;
         this.myPayService = myPayService;
      }

      [HttpPost]
      public async Task<ActionResult> Post([FromBody]PaymentRequest request)
      {
         try
         {
            await this.myPayService.RequestPayment(request);
            return Ok();
         }
         catch (Exception e)
         {
            this.logger.LogError(e.ToString());
            return BadRequest();
         }
      }

      [HttpGet("{requestId}/state")]
      public async Task<ActionResult<string>> GetStatus(string requestId)
      {
         try
         {
            var paymentStatus = await this.myPayService.GetStatus(requestId);
            var report = paymentStatus.Map(x => x switch
            {
               PaymentStatus.Pending => "pending",
               PaymentStatus.Completed => "completed",
               PaymentStatus.Cancelled => "cancelled",
               _ => throw new ArgumentException()
            });

            return report.Match<ActionResult<string>>(
               some: status =>
               {
                  this.logger.LogInformation($"Payment with request id {requestId} has status {status}.");
                  return Ok(status);
               },
               none: () => NotFound());
         }
         catch (Exception e)
         {
            this.logger.LogError(e.ToString());
            return BadRequest();
         }
      }
   }
}
