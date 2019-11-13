using System;
using System.IO;
using Common.Aggregate;

namespace Domain.Payment
{
   public class PayForOrder_v2 : Command
   {
      public PayForOrder_v2(string correlationId, DateTime timeStamp) : base(correlationId, timeStamp)
      {
      }

      public string OrderStreamId { get; set; }
      public decimal Total { get; set; }
      public string Description { get; set; }

      private void Validate()
      {
         if (string.IsNullOrWhiteSpace(OrderStreamId)) throw new InvalidDataException(nameof(OrderStreamId));
         if (Total <= 0) throw new InvalidDataException(nameof(Total));
         if (string.IsNullOrWhiteSpace(Description)) throw new InvalidDataException(nameof(Description));
      }

      public PaymentRequested Execute(Payment_v2 state)
      {
         Validate();
         return CreateEvent(state);
      }

      private PaymentRequested CreateEvent(Payment_v2 state)
      {
         var evn = CreateEvent<PaymentRequested>(state);
         evn.OrderStreamId = OrderStreamId;
         evn.Total = Total;
         evn.Description = Description;
         return evn;
      }
   }
}