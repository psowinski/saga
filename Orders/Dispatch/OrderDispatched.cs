using System.Collections.Generic;
using System.Linq;
using Domain.Common;
using Domain.Orders;

namespace Domain.Dispatch
{
   public class OrderDispatched : SagaEvent<Dispatch>
   {
      public OrderDispatched()
      {
      }

      public OrderDispatched(Dispatch state, DispatchOrder command) : base(state, command)
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