using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;

namespace Runner
{
   public class IndexEventPerCategory
   {
      public IndexEventPerCategory(string category, ByCategoryIndexEvent @event)
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
      private readonly SagaConfiguration configuration;
      private readonly Dictionary<string, int> categoryState = new Dictionary<string, int>();

      public SagaManager(SagaConfiguration configuration)
      {
         this.configuration = configuration;
         InitializeCategoryState();
      }

      private void InitializeCategoryState()
      {
         foreach (var category in this.configuration.Categories)
            this.categoryState.Add(category, 0);
      }

      public async Task Run()
      {
         var indexEvents = await LoadIndex();
         await ContinueUnfinishedSagas(indexEvents);
      }

      private async Task<IEnumerable<IndexEventPerCategory>> LoadIndex()
      {
         var categoryIndices = await Task.WhenAll(this.categoryState.Select(LoadCategoryIndex).ToList());
         var events = categoryIndices.Aggregate((x, y) => x.Union(y));
         return events;
      }

      private async Task<IEnumerable<IndexEventPerCategory>> LoadCategoryIndex(KeyValuePair<string, int> reg)
      {
         var (category, lastProcessedVersion) = reg;
         var lastVersion = await this.persistence.GetLastStreamVersion(this.persistence.GetCategoryStreamId(category));
         if (lastProcessedVersion >= lastVersion) return Enumerable.Empty<IndexEventPerCategory>();

         PrintCategoryStatus(reg, lastVersion);

         var categoryIndexEvents = await LoadCategoryIndexEvents(reg.Key, lastProcessedVersion + 1);
         UpdateCategoryState(category, lastVersion);
         return categoryIndexEvents;
      }

      public void UpdateCategoryState(string category, int version) => this.categoryState[category] = version;

      private void PrintCategoryStatus(KeyValuePair<string, int> reg, int lastVersion)
      {
         Console.WriteLine(
            $"Processing {lastVersion - reg.Value} event(s) of category [{reg.Key}] (ver. {reg.Value + 1}-{lastVersion}).");
      }

      private async Task<IEnumerable<IndexEventPerCategory>> LoadCategoryIndexEvents(string category, int fromVersion)
      {
         var indexEvents = await this.persistence.Load(this.persistence.GetCategoryStreamId(category), fromVersion);
         return indexEvents.Select(x => x is ByCategoryIndexEvent evn ? new IndexEventPerCategory(category, evn) : null).Where(x => x != null);
      }

      private async Task ContinueUnfinishedSagas(IEnumerable<IndexEventPerCategory> events)
      {
         var sagasContinuation = CreateSagasContinuation(events);
         await ContinueSagasExecution(sagasContinuation);
      }

      private IEnumerable<dynamic> CreateSagasContinuation(IEnumerable<IndexEventPerCategory> events)
      {
         var eventsToProcess = events
            .Where(AcceptOnlySagaEventTypes)
            .GroupBy(x => x.Event.RefCorrelationId)
            .Select(g => g.OrderByDescending(e => e.Event.RefTimeStamp).First())
            .Select(lastEvn => CreateSagaTask(lastEvn))
               .Where(task => task != null);
         return eventsToProcess;
      }

      private bool AcceptOnlySagaEventTypes(IndexEventPerCategory evn)
         => this.configuration.IsKnownEventType(evn.Category, evn.Event.RefType);

      private dynamic CreateSagaTask(IndexEventPerCategory indexEventPerCategory)
      {
         var saga = this.configuration.GetSagaAction(indexEventPerCategory.Category, indexEventPerCategory.Event.RefType);
         return saga == null ? null : new { indexEventPerCategory.Event.RefStreamId, indexEventPerCategory.Event.RefVersion, saga};
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
