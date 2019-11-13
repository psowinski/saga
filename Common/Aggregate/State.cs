using System;
using System.Collections.Generic;

namespace Common.Aggregate
{
   public abstract class State
   {
      protected State(string streamId)
      {
         if (string.IsNullOrWhiteSpace(streamId)) throw new ArgumentException(nameof(streamId));

         StreamId = streamId;
         Version = 0;
      }

      public string StreamId { get; }

      public int Version { get; private set; }

      protected void ApplyVersion(Event evn)
      {
         if (this.Version + 1 != evn.Version) throw new ArgumentException(nameof(evn.Version));
         if (this.StreamId != evn.StreamId) throw new ArgumentException(nameof(evn.StreamId));

         this.Version += 1;
      }

      public abstract void Apply(Event evn);

      public void Apply(IEnumerable<Event> events)
      {
         foreach (var evn in events)
            Apply(evn);
      }
   }
}