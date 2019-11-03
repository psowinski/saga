namespace Common.Aggregate
{
   public abstract class AggregateEvent<T> : Event where T : State
   {
      public static string Category => typeof(T).Name.ToLower();
   }
}
