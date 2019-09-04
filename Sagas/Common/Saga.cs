using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Infrastructure;
using Infrastructure.Service;

namespace Sagas.Common
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

   public class Saga : ITask
   {
      private readonly Persistence persistence= new Persistence();
      private readonly SagaConfiguration configuration;
      private List<(string Category, int Version)> categoryState;

      public Saga(SagaConfiguration configuration)
      {
         this.configuration = configuration;
         InitializeCategoryState();
      }

      private void InitializeCategoryState()
      {
         this.categoryState = this.configuration.Categories
            .Select(category => (category, 0))
            .ToList();
      }

      public async Task Run()
      {
         var (indexEvents, categoryState) = await LoadIndex();
         await ContinueUnfinishedSagas(indexEvents);
         UpdateCategoryState(categoryState);
      }

      private async Task<(IEnumerable<IndexEventPerCategory> IndexEvents, IEnumerable<(string Category, int Version)> CategoryState)> LoadIndex()
      {
         var categoryIndices = await Task.WhenAll(this.categoryState.Select(LoadCategoryIndex).ToList());
         var indexEvents = categoryIndices.Select(x => x.IndexEvents).Aggregate((x, y) => x.Union(y));
         var newCategoryState = categoryIndices.Select(x => (x.Category, x.CategoryLastVersion));
         return (indexEvents, newCategoryState);
      }

      private async Task<(IEnumerable<IndexEventPerCategory> IndexEvents, string Category, int CategoryLastVersion)> LoadCategoryIndex((string Category, int Version) categoryWithVersion)
      {
         var lastVersion = await this.persistence.GetLastStreamVersion(this.persistence.GetCategoryStreamId(categoryWithVersion.Category));
         if (categoryWithVersion.Version >= lastVersion) return (Enumerable.Empty<IndexEventPerCategory>(), categoryWithVersion.Category, lastVersion);

         PrintCategoryStatus(categoryWithVersion, lastVersion);

         var categoryIndexEvents = await LoadCategoryIndexEvents(categoryWithVersion.Category, categoryWithVersion.Version + 1);
         return (categoryIndexEvents, categoryWithVersion.Category, lastVersion);
      }

      public void UpdateCategoryState(IEnumerable<(string category, int version)> newCategoryState) =>
         this.categoryState = newCategoryState.ToList();

      private void PrintCategoryStatus((string Category, int Version) categoryWithVersion, int lastVersion)
      {
         Console.WriteLine(
            $"Processing {lastVersion - categoryWithVersion.Version} event(s) of category [{categoryWithVersion.Category}] (ver. {categoryWithVersion.Version + 1}-{lastVersion}).");
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

      private IEnumerable<(IndexEventPerCategory Index, ISagaAction Action)> CreateSagasContinuation(IEnumerable<IndexEventPerCategory> events)
      {
         var eventsToProcess = events
            .Where(AcceptOnlySagaEventTypes)
            .GroupBy(x => x.Event.RefCorrelationId)
            .Select(g => g.OrderByDescending(e => e.Event.RefTimeStamp).First())
            .Select(FindActionsForEvents)
               .Where(a => a.Action != null);
         return eventsToProcess;
      }

      private bool AcceptOnlySagaEventTypes(IndexEventPerCategory evn)
         => this.configuration.IsKnownEventType(evn.Category, evn.Event.RefType);

      private (IndexEventPerCategory Index, ISagaAction Action) FindActionsForEvents(IndexEventPerCategory indexEventPerCategory)
      {
         var sagaAction = this.configuration.GetSagaAction(indexEventPerCategory.Category, indexEventPerCategory.Event.RefType);
         return (indexEventPerCategory, sagaAction);
      }

      private async Task ContinueSagasExecution(IEnumerable<(IndexEventPerCategory Index, ISagaAction Action)> sagaTasks)
      {
         foreach (var (index, action) in sagaTasks)
         {
            var refEvent = await this.persistence.LoadEvent(index.Event.RefStreamId, index.Event.RefVersion);
            await action.ProcessEvent(refEvent);
         }
      }
   }
}
