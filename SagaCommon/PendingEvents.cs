using System;
using System.Collections.Generic;
using System.Linq;
using Common.Aggregate;

namespace Saga
{
   public class PendingEvents
   {
      private readonly TimeSpan firstDelay = new TimeSpan(0, 0, 0, 0, 100);
      private readonly TimeSpan maxDelay = new TimeSpan(0, 5, 0);

      private readonly Dictionary<Indexed, (DateTime WaitUntil, TimeSpan LastDelay)> events = 
         new Dictionary<Indexed, (DateTime WaitUntil, TimeSpan LastDelay)>();

      public ActionStatus ProcessActionStatus(ActionStatus status, Indexed evn)
      {
         if (status == ActionStatus.Pending && !Add(evn))
            return ActionStatus.Error;
         if (status == ActionStatus.Ok)
            Remove(evn);
         return status;
      }

      private bool Add(Indexed evn)
      {
         if(this.events.TryGetValue(evn, out var info))
         {
            var delay = info.LastDelay * 2;
            if (delay >= maxDelay) return false;
            this.events[evn] = (info.WaitUntil.Add(delay), delay);
         }
         else
            this.events.Add(evn, (DateTime.Now.Add(firstDelay), firstDelay));

         return true;
      }

      private void Remove(Indexed evn)
         => this.events.Remove(evn);

      public void ClampToState(CategoryState state)
         => this.events
            .Select(x => x.Key)
            .Where(evn => !state.IsInScope(evn))
            .ToList()
            .ForEach(x => this.events.Remove(x));

      public IEnumerable<Indexed> GetReadyEvents()
         => this.events
            .Where(x => DateTime.Now >= x.Value.WaitUntil)
            .Select(x => x.Key);
   }
}