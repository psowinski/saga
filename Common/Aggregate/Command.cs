using System;

namespace Common.Aggregate
{
   public class Command
   {
      public Command(string correlationId, DateTime timeStamp)
      {
         CorrelationId = correlationId;
         TimeStamp = timeStamp;
      }

      public string CorrelationId { get; }
      public DateTime TimeStamp { get; }

      protected T CreateEvent<T>(State state) where T : Event, new()
      {
         var evn = new T
         {
            StreamId = state.StreamId,
            Version = state.Version + 1,
            CorrelationId = CorrelationId,
            TimeStamp = TimeStamp
         };
         return evn;
      }
   }
}