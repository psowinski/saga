namespace Domain
{
   public interface IAggregate<T>
   {
      T Zero(string streamId);
      T Apply(T state, Event evn);
   }
}