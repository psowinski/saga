using System.Collections.Generic;
using Common.Aggregate;

namespace Domain.Model.Dispatch
{
   public class OrderDispatched : AggregateEvent<Dispatch>
   {
      public string PaymentStreamId { get; set; }
      public string OrderStreamId { get; set; }
      public IEnumerable<string> Items { get; set; }
   }
}