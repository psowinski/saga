using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Aggregate;

namespace Persistence
{
   public interface IPersistenceClient
   {
      string CreateCategoryIndexStreamId(string category);
      Task<TState> GetState<TState, TUpdater>(string streamId) 
         where TState : State where TUpdater : IEventUpdater<TState>, new();
      Task<List<T>> Load<T>(string streamId);
      Task<List<T>> Load<T>(string streamId, int fromVersion);
      Task<List<T>> Load<T>(string streamId, int fromVersion, int toVersion);
      Task<T> LoadEvent<T>(string streamId, int version);
      Task Save<T>(IEnumerable<T> events);
      Task Save<T>(T evn);
   }
}