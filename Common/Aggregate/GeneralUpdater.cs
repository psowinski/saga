using System.Collections.Generic;

namespace Common.Aggregate
{
   public class GeneralUpdater<T> : IEventUpdater<T> where T : State
   {
      public IEnumerable<AggregateEvent<T>> Update(Event evn)
      {
         yield return (AggregateEvent<T>)evn;
      }
   }
}