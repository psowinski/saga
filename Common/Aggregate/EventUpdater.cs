using System;
using System.Collections.Generic;

namespace Common.Aggregate
{
   public class EventUpdater<T> where T : State
   {
      public virtual IEnumerable<AggregateEvent<T>> Update(AggregateEvent<T> evn)
      { 
         yield return evn;
      }

      protected U CopyEvent<U>(AggregateEvent<T> evn) where U : Event, new()
      {
         var copied = new U
         {
            StreamId = evn.StreamId,
            Version = evn.Version,
            CorrelationId = evn.CorrelationId,
            TimeStamp = evn.TimeStamp
         };
         return copied;
      }
   }
}
