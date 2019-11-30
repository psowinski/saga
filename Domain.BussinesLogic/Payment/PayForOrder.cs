using System;
using System.IO;
using Common.Aggregate;
using Domain.Model.Payment;

namespace Domain.BusinessLogic.Payment
{
   public class PayForOrder : Command
   {
      public PayForOrder(string correlationId, DateTime timeStamp) : base(correlationId, timeStamp)
      {
      }

      public string OrderStreamId { get; set; }
      public decimal Total { get; set; }
      public string Description { get; set; }

      private void Validate(Model.Payment.Payment state)
      {
         if (string.IsNullOrWhiteSpace(OrderStreamId)) throw new InvalidDataException(nameof(OrderStreamId));
         if (Total <= 0) throw new InvalidDataException(nameof(Total));
         if (string.IsNullOrWhiteSpace(Description)) throw new InvalidDataException(nameof(Description));

         if(state.Status != PaymentStatus.Unpaid) throw new InvalidDataException(nameof(state.Status));
      }

      public PaymentRequested Execute(Model.Payment.Payment state)
      {
         Validate(state);
         return CreateEvent(state);
      }

      private PaymentRequested CreateEvent(Model.Payment.Payment state)
      {
         var evn = CreateEvent<PaymentRequested>(state);
         evn.OrderStreamId = OrderStreamId;
         evn.Total = Total;
         evn.Description = Description;
         return evn;
      }
   }
}