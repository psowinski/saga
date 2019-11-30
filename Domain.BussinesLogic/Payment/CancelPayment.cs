using System;
using System.IO;
using Common.Aggregate;
using Domain.Model.Payment;

namespace Domain.BusinessLogic.Payment
{
   public class CancelPayment : Command
   {
      public CancelPayment(string correlationId, DateTime timeStamp) : base(correlationId, timeStamp)
      {
      }

      private void Validate(Model.Payment.Payment state)
      {
         if (state.Status != PaymentStatus.Pending) throw new InvalidDataException(nameof(state.Status));
      }

      public PaymentCancelled Execute(Model.Payment.Payment state)
      {
         Validate(state);
         return CreateEvent<PaymentCancelled>(state);
      }
   }
}