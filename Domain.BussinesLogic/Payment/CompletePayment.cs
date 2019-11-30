using System;
using System.IO;
using Common.Aggregate;
using Domain.Model.Payment;

namespace Domain.BusinessLogic.Payment
{
   public class CompletePayment : Command
   {
      public CompletePayment(string correlationId, DateTime timeStamp) : base(correlationId, timeStamp)
      {
      }

      private void Validate(Model.Payment.Payment state)
      {
         if (state.Status != PaymentStatus.Pending) throw new InvalidDataException(nameof(state.Status));
      }

      public PaymentCompleted Execute(Model.Payment.Payment state)
      {
         Validate(state);
         var evn = CreateEvent<PaymentCompleted>(state);
         evn.OrderStreamId = state.OrderStreamId;
         return evn;
      }
   }
}