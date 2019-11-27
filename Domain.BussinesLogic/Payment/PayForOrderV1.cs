using System;
using System.IO;
using Common.Aggregate;

namespace Domain.Payment
{
   public class PayForOrderV1 : Command
   {
      public PayForOrderV1(string correlationId, DateTime timeStamp) : base(correlationId, timeStamp)
      {
      }

      public string OrderStreamId { get; set; }
      public decimal Amount { get; set; }

      private void Validate()
      {
         if (string.IsNullOrWhiteSpace(OrderStreamId)) throw new InvalidDataException(nameof(OrderStreamId));
         if (Amount <= 0) throw new InvalidDataException(nameof(Amount));
      }

      public OrderPaid Execute(PaymentV1 state)
      {
         Validate();
         return CreateEvent(state);
      }

      private OrderPaid CreateEvent(PaymentV1 state)
      {
         var evn = CreateEvent<OrderPaid>(state);
         evn.OrderStreamId = OrderStreamId;
         evn.Amount = Amount;
         return evn;
      }
   }
}