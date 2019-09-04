using System;
using System.IO;
using Domain.Common;

namespace Domain.Payment
{
   public class PayCommand : Command
   {
      public PayCommand(string correlationId, DateTime timeStamp) : base(correlationId, timeStamp)
      {
      }

      public string OrderStreamId;
      public decimal Amount;

      private void Validate()
      {
         if (string.IsNullOrWhiteSpace(OrderStreamId)) throw new InvalidDataException(nameof(OrderStreamId));
         if (Amount >= 0) throw new InvalidDataException(nameof(Amount));
      }

      public PaidEvent Execute(PaymentState state)
      {
         Validate();
         return new PaidEvent(state, this);
      }
   }
}