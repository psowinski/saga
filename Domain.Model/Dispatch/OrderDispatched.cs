using System.Collections.Generic;
using System.Linq;
using Common.Aggregate;
using Domain.Order;

namespace Domain.Dispatch
{
   public class OrderDispatched : AggregateEvent<Dispatch>
   {
      public string PaymentStreamId { get; set; }
      public string OrderStreamId { get; set; }
      public IEnumerable<string> Items { get; set; }
   }
}