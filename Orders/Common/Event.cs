namespace Domain.Common
{
   public abstract class Event
   {
      protected Event()
      {
      }

      protected Event(State state, Command command)
      {
         StreamId = state.StreamId;
         Version = state.Version + 1;
      }

      public string StreamId { get; set; }
      public int Version { get; set; }
   }
}