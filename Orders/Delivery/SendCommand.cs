using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Domain.Common;

namespace Domain.Delivery
{
   public class SendCommand : Command
   {
      public SendCommand(string correlationId, DateTime timeStamp) : base(correlationId, timeStamp)
      {
      }

      public string PaymentStreamId;
      public string OrderStreamId;
      public IEnumerable<string> Items;

      private void Validate()
      {
         if (string.IsNullOrWhiteSpace(PaymentStreamId)) throw new InvalidDataException(nameof(PaymentStreamId));
         if (string.IsNullOrWhiteSpace(OrderStreamId)) throw new InvalidDataException(nameof(OrderStreamId));
         if (!Items.Any()) throw new InvalidDataException(nameof(Items));
      }

      public SendEvent Execute(DeliveryState state)
      {
         Validate();
         return new SendEvent(state, this);
      }
   }
}