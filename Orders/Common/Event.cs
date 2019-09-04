using System;

namespace Domain.Common
{
   public abstract class Event
   {
      protected Event(State state, Command command)
      {
         StreamId = state.StreamId;
         Version = state.Version + 1;

         CorrelationId = command.CorrelationId;
         TimeStamp = command.TimeStamp;
      }

      public string CorrelationId { get; set; }
      public DateTime TimeStamp { get; set; }

      public string StreamId { get; set; }
      public int Version { get; set; }
   }
}