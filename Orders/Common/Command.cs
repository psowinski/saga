using System;

namespace Domain.Common
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
   }
}