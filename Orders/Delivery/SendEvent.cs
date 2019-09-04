using System.Collections.Generic;
using System.Linq;
using Domain.Common;

namespace Domain.Delivery
{
   public class SendEvent : Event
   {
      public SendEvent(DeliveryState state, SendCommand command) : base(state, command)
      {
         PaymentStreamId = command.PaymentStreamId;
         OrderStreamId = command.OrderStreamId;
         Items = command.Items.ToList();
      }

      public string PaymentStreamId { get; set; }
      public string OrderStreamId { get; set; }
      public IEnumerable<string> Items { get; set; }
   }
}