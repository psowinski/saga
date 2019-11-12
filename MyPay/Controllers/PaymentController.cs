using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.General;
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
         await this.myPayService.RequestPayment(request);
         return Ok();
      }

      [HttpGet("status/{requestId}")]
      public async Task<ActionResult<string>> GetStatus(string requestId)
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
            some: status => {
               this.logger.LogInformation($"Payment with request id {requestId} has status {status}.");
               return Ok(status);
            },
            none: () => BadRequest());
      }
   }
}
