using System;
using System.Collections.Generic;

namespace Common.Aggregate
{
   public interface IEventUpdater<T> where T : State
   {
      //evn has general type because of C# lack of good contravariant support
      IEnumerable<AggregateEvent<T>> Update(Event evn);
   }
}
