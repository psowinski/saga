using System;
using System.IO;
using Common.Aggregate;

namespace Domain.Payment
{
   public class PayForOrder : Command
   {
      public PayForOrder(string correlationId, DateTime timeStamp) : base(correlationId, timeStamp)
      {
      }

      public string OrderStreamId { get; set; }
      public decimal Amount { get; set; }

      private void Validate()
      {
         if (string.IsNullOrWhiteSpace(OrderStreamId)) throw new InvalidDataException(nameof(OrderStreamId));
         if (Amount <= 0) throw new InvalidDataException(nameof(Amount));
      }

      public OrderPaid Execute(Payment state)
      {
         Validate();
         return CreateEvent(state);
      }

      private OrderPaid CreateEvent(Payment state)
      {
         var evn = CreateEvent<OrderPaid>(state);
         evn.OrderStreamId = OrderStreamId;
         evn.Amount = Amount;
         return evn;
      }
   }
}