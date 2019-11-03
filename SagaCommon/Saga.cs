using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Aggregate;
using Persistence;

namespace Saga
{
   public class Saga
   {
      private readonly IPersistenceClient persistence;
      private readonly SagaConfiguration configuration;
      private List<(string Category, int Version)> state;

      public Saga(SagaConfiguration configuration, IPersistenceClient persistence)
      {
         this.configuration = configuration;
         this.persistence = persistence;
         InitializeCategoryState();
      }

      private void InitializeCategoryState()
      {
         this.state = this.configuration.Categories
            .Select(category => (category, 0))
            .ToList();
      }

      public async Task Run()
      {
         var byCategoryIndexes = await LoadTrackedByCategoryIndexes();
         await ContinueUnfinishedSagas(byCategoryIndexes.SelectMany(x => x));
         PrintNewState(byCategoryIndexes);
         UpdateState(byCategoryIndexes);
      }

      private Task<List<Indexed>[]> LoadTrackedByCategoryIndexes() => Task.WhenAll(this.state.Select(LoadByCategoryIndex).ToList());

      public string CategoryNameIndexStreamId(string streamId) =>
         streamId.Substring(streamId.IndexOf('-') + 1);

      private Task<List<Indexed>> LoadByCategoryIndex((string Category, int Version) x)
         => this.persistence.Load<Indexed>(this.persistence.CreateCategoryIndexStreamId(x.Category), x.Version + 1);

      public void UpdateState(List<Indexed>[] byCategoryIndexes)
      {
         var updated = GetUpdatedState(byCategoryIndexes);
         var missing = GetMissingState(updated);
         this.state = updated.Union(missing).ToList();
      }

      private IEnumerable<(string Category, int Version)> GetMissingState(IEnumerable<(string Category, int Version)> updated)
         => this.state.Where(x => updated.All(y => y.Category != x.Category));

      private List<(string Category, int Version)> GetUpdatedState(List<Indexed>[] byCategoryIndexes)
      {
         var updated = byCategoryIndexes
            .Where(x => x.Count > 0)
            .Select(x => (Category: CategoryNameIndexStreamId(x.First().StreamId), Version: x.Max(y => y.Version)))
            .ToList();
         return updated;
      }
 
      public void PrintNewState(List<Indexed>[] byCategoryIndexes)
      {
         var updated = GetUpdatedState(byCategoryIndexes);
         PrintUpdatedState(updated);
      }

      public static bool ShowSagaReports = false;
      private void PrintUpdatedState(IEnumerable<(string Category, int Version)> updated)
      {
         if (!ShowSagaReports) return;

         foreach (var (category, version) in updated)
         {
            var prevVersion = this.state.First(x => x.Category == category).Version;
            Console.WriteLine(
               $"Processing {version - prevVersion} event(s) of category [{category}] (ver. {prevVersion + 1}-{version}).");
         }
      }

      private async Task ContinueUnfinishedSagas(IEnumerable<Indexed> events)
      {
         var sagasContinuation = CreateSagasContinuation(events);
         await ContinueSagas(sagasContinuation);
      }

      private IEnumerable<(Indexed Event, Func<Event, Task> Action)> CreateSagasContinuation(IEnumerable<Indexed> index)
      {
         var eventsToProcess = index
            .Where(KeepSagaEvents)
            .GroupBy(x => x.RefCorrelationId)
            .Select(g => BindActionToEvent(LastGroupEvent(g)));
         return eventsToProcess;
      }

      private bool KeepSagaEvents(Indexed evn)
         => this.configuration.IsKnownEventType(evn.RefType);

      private Indexed LastGroupEvent(IGrouping<string, Indexed> group)
         => group.OrderByDescending(e => e.RefTimeStamp).First();

      private (Indexed Event, Func<Event, Task> Action) BindActionToEvent(Indexed evn)
         => (evn, this.configuration.GetAction(evn.RefType));

      private Task ContinueSagas(IEnumerable<(Indexed Event, Func<Event, Task> Action)> sagaTasks)
         => Task.WhenAll(sagaTasks.Select(x => ContinueSagaTask(x.Event, x.Action)).ToList());

      private async Task ContinueSagaTask(Indexed evn, Func<Event, Task> action)
      {
         var refEvent = await this.persistence.LoadEvent<Event>(evn.RefStreamId, evn.RefVersion);
         await action(refEvent);
      }
   }
}
