using System;

namespace Orders
{
   public class State
   {
      public State(string streamId)
      {
         StreamId = streamId;
      }

      public string StreamId { get; }
      public int Version { get; protected set; }

      protected void Apply(Event evn)
      {
         if (++Version != evn.Version) throw new ArgumentException(nameof(Version));
         if (StreamId != evn.StreamId) throw new ArgumentException(nameof(StreamId));
      }
   }
}