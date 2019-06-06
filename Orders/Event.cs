namespace Orders
{
   public class Event
   {
      public Event(string streamId, int version)
      {
         StreamId = streamId;
         Version = version;
      }

      public string Type { get; protected set; }
      public string StreamId { get; }
      public int Version { get; }
   }
}