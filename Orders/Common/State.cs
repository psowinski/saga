using System;

namespace Domain.Common
{
   public abstract class State
   {
      protected State(string streamId)
      {
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
   }
}