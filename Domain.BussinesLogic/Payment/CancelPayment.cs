using System;
using System.IO;
using Common.Aggregate;

namespace Domain.Payment
{
   public class CancelPayment : Command
   {
      public CancelPayment(string correlationId, DateTime timeStamp) : base(correlationId, timeStamp)
      {
      }

      private void Validate(PaymentV2 state)
      {
         if (state.Status != PaymentStatus.Pending) throw new InvalidDataException(nameof(state.Status));
      }

      public PaymentCancelled Execute(PaymentV2 state)
      {
         Validate(state);
         return CreateEvent<PaymentCancelled>(state);
      }
   }
}