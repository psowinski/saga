using System;
using System.IO;
using Common.Aggregate;

namespace Domain.Payment
{
   public class CompletePayment : Command
   {
      public CompletePayment(string correlationId, DateTime timeStamp) : base(correlationId, timeStamp)
      {
      }

      private void Validate(PaymentV2 state)
      {
         if (state.Status != PaymentStatus.Pending) throw new InvalidDataException(nameof(state.Status));
      }

      public PaymentCompleted Execute(PaymentV2 state)
      {
         Validate(state);
         var evn = CreateEvent<PaymentCompleted>(state);
         evn.OrderStreamId = state.OrderStreamId;
         return evn;
      }
   }
}