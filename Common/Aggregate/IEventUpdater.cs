using System.Collections.Generic;

namespace Common.Aggregate
{
   public interface IEventUpdater
   {
      IEnumerable<Event> Update(Event evn);
   }
}
