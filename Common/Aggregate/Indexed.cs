using System;
using Common.Aggregate;

namespace Common.Aggregate
{
   public class Indexed : Event
   {
      public string RefStreamId { get; set; }
      public int RefVersion { get; set; }
      public string RefType { get; set; }
      public string RefCorrelationId { get; set; }
      public DateTime RefTimeStamp { get; set; }

      private string category;
      public string Category => this.category ??= StreamId.Substring(StreamId.IndexOf('-') + 1);
   }
}