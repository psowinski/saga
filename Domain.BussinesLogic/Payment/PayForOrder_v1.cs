using System;
using System.IO;
using Common.Aggregate;

namespace Domain.Payment
{
   public class PayForOrder_v1 : Command
   {
      public PayForOrder_v1(string correlationId, DateTime timeStamp) : base(correlationId, timeStamp)
      {
      }

      public string OrderStreamId { get; set; }
      public decimal Amount { get; set; }

      private void Validate()
      {
         if (string.IsNullOrWhiteSpace(OrderStreamId)) throw new InvalidDataException(nameof(OrderStreamId));
         if (Amount <= 0) throw new InvalidDataException(nameof(Amount));
      }

      public OrderPaid Execute(Payment_v1 state)
      {
         Validate();
         return CreateEvent(state);
      }

      private OrderPaid CreateEvent(Payment_v1 state)
      {
         var evn = CreateEvent<OrderPaid>(state);
         evn.OrderStreamId = OrderStreamId;
         evn.Amount = Amount;
         return evn;
      }
   }
}