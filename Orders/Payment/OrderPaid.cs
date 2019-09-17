using Domain.Common;

namespace Domain.Payment
{
   public class OrderPaid : SagaEvent<Payment>
   {
      public OrderPaid()
      {
      }

      public OrderPaid(Payment state, PayForOrder command) : base(state, command)
      {
         OrderStreamId = command.OrderStreamId;
         Amount = command.Amount;
      }

      public string OrderStreamId { get; set; }
      public decimal Amount { get; set; }
   }
}