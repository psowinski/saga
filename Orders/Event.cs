namespace Orders
{
   public abstract class Event
   {
      public string StreamId = "";
      public int Version = 0;
   }
}