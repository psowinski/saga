using System;

namespace Domain.Common
{
   public class Indexed : Event
   {
      public Indexed()
      {
      }

      public Indexed(State state, Command command) : base(state, command)
      {
      }

      public string RefStreamId { get; set; }
      public int RefVersion { get; set; }
      public string RefType { get; set; }
      public string RefCorrelationId { get; set; }
      public DateTime RefTimeStamp { get; set; }
   }
}