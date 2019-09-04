using Domain.Common;

namespace Domain.Payment
{
   public class PaidEvent : Event
   {
      public PaidEvent(PaymentState state, PayCommand command) : base(state, command)
      {
         OrderStreamId = command.OrderStreamId;
         Amount = command.Amount;
      }

      public string OrderStreamId { get; set; }
      public decimal Amount { get; set; }
   }
}