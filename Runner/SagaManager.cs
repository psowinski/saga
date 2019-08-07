using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;

namespace Runner
{
   public class CategoryProcess
   {
      public int LastProcessedVersion;
      public HashSet<ValueTuple<string, ISaga>> EventTypes;
   }

   public class CategoryIndexEvent
   {
      public CategoryIndexEvent(string category, ByCategoryIndexEvent @event)
      {
         Category = category;
         Event = @event;
      }

      public string Category;
      public ByCategoryIndexEvent Event;
   }

   public class SagaManager : IServiceTask
   {
      private readonly Persistence persistence= new Persistence();

      private readonly Dictionary<string, CategoryProcess> register = new Dictionary<string, CategoryProcess>();
      public void RegisterSaga(string category, string eventType, ISaga saga)
      {
         if (this.register.TryGetValue(category, out var index))
            index.EventTypes.Add((eventType, saga));
         else
            this.register.Add(category, new CategoryProcess {EventTypes = new HashSet<ValueTuple<string, ISaga>> {(eventType, saga)}});
      }

      public void RegisterSagaEnd(string category)
      {
         if (!this.register.TryGetValue(category, out var index))
            this.register.Add(category, new CategoryProcess {EventTypes = new HashSet<ValueTuple<string, ISaga>>()});
      }

      public async Task Run()
      {
         var indexEvents = await LoadIndex();
         await ContinueUnfinishedSagas(indexEvents);
      }

      private async Task<IEnumerable<CategoryIndexEvent>> LoadIndex()
      {
         var categoryIndices = await Task.WhenAll(this.register.Select(LoadCategoryIndex).ToList());
         var events = categoryIndices.Aggregate((x, y) => x.Union(y));
         return events;
      }

      private async Task<IEnumerable<CategoryIndexEvent>> LoadCategoryIndex(KeyValuePair<string, CategoryProcess> reg)
      {
         var lastVersion = await this.persistence.GetLastStreamVersion(this.persistence.GetCategoryStreamId(reg.Key));
         if (reg.Value.LastProcessedVersion >= lastVersion) return Enumerable.Empty<CategoryIndexEvent>();

         PrintCategoryStatus(reg, lastVersion);

         var categoryIndexEvents = await LoadCategoryIndexEvents(reg.Key, reg.Value.LastProcessedVersion + 1);
         reg.Value.LastProcessedVersion = lastVersion;
         return categoryIndexEvents;
      }

      private static void PrintCategoryStatus(KeyValuePair<string, CategoryProcess> reg, int lastVersion)
      {
         Console.WriteLine(
            $"Processing {lastVersion - reg.Value.LastProcessedVersion} event(s) of category [{reg.Key}] (ver. {reg.Value.LastProcessedVersion + 1}-{lastVersion}).");
      }

      private async Task<IEnumerable<CategoryIndexEvent>> LoadCategoryIndexEvents(string category, int fromVersion)
      {
         var indexEvents = await this.persistence.Load(this.persistence.GetCategoryStreamId(category), fromVersion);
         return indexEvents.Select(x => x is ByCategoryIndexEvent evn ? new CategoryIndexEvent(category, evn) : null).Where(x => x != null);
      }

      private async Task ContinueUnfinishedSagas(IEnumerable<CategoryIndexEvent> events)
      {
         var sagasContinuation = CreateSagasContinuation(events);
         await ContinueSagasExecution(sagasContinuation);
      }

      private IEnumerable<dynamic> CreateSagasContinuation(IEnumerable<CategoryIndexEvent> events)
      {
         var eventsToProcess = events
            .GroupBy(x => x.Event.RefCorrelationId)
            .Select(g => g.OrderByDescending(e => e.Event.RefTimeStamp).First())
            .Select(lastEvn => CreateSagaTask(lastEvn))
               .Where(task => task != null);
         return eventsToProcess;
      }

      private dynamic CreateSagaTask(CategoryIndexEvent categoryEvent)
      {
         var saga = this.register[categoryEvent.Category].EventTypes
            .FirstOrDefault(e => e.Item1 == categoryEvent.Event.RefType).Item2;

         return saga == null ? null : new { categoryEvent.Event.RefStreamId, categoryEvent.Event.RefVersion, saga};
      }

      private async Task ContinueSagasExecution(IEnumerable<dynamic> sagaTasks)
      {
         foreach (var sagaTask in sagaTasks)
         {
            var refEvent = await this.persistence.LoadEvent(sagaTask.RefStreamId, sagaTask.RefVersion);
            await sagaTask.saga.ProcessEvent(refEvent);
         }
      }
   }
}
