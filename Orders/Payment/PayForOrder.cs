using System;
using System.IO;
using Domain.Common;

namespace Domain.Payment
{
   public class PayForOrder : Command
   {
      public PayForOrder(string correlationId, DateTime timeStamp) : base(correlationId, timeStamp)
      {
      }

      public string OrderStreamId;
      public decimal Amount;

      private void Validate()
      {
         if (string.IsNullOrWhiteSpace(OrderStreamId)) throw new InvalidDataException(nameof(OrderStreamId));
         if (Amount <= 0) throw new InvalidDataException(nameof(Amount));
      }

      public OrderPaid Execute(Payment state)
      {
         Validate();
         return new OrderPaid(state, this);
      }
   }
}