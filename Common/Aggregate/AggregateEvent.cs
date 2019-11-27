namespace Common.Aggregate
{
   public abstract class AggregateEvent<T> : Event where T : State
   {
      public static string Category => typeof(T).Name.ToLower();

      public U CopyTo<U>() where U : Event, new()
      {
         var copied = new U
         {
            StreamId = this.StreamId,
            Version = this.Version,
            CorrelationId = this.CorrelationId,
            TimeStamp = this.TimeStamp
         };
         return copied;
      }
   }
}
