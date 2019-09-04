using System;
using Domain.Common;

namespace Domain
{
   public class ByCategoryIndexEvent : Event
   {
      public ByCategoryIndexEvent(State state, Command command) : base(state, command)
      {
      }

      public string RefStreamId { get; set; }
      public int RefVersion { get; set; }
      public string RefType { get; set; }
      public string RefCorrelationId { get; set; }
      public DateTime RefTimeStamp { get; set; }
   }
}