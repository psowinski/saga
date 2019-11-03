using System;

namespace Common.Aggregate
{
   public abstract class Event
   {
      public string StreamId { get; set; }
      public int Version { get; set; }
      
      public string CorrelationId { get; set; }
      public DateTime TimeStamp { get; set; }

      public override string ToString()
      {
         return $"{StreamId}|{Version}";
      }
   }
}