using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Aggregate;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Saga
{
   public class Saga
   {
      private readonly IPersistenceClient persistence;
      private readonly SagaConfiguration configuration;
      private readonly HashSet<Indexed> pendingEvents = new HashSet<Indexed>();

      private readonly CategoryState state;

      private readonly ILogger logger;

      public Saga(SagaConfiguration configuration, IPersistenceClient persistence, ILogger logger)
      {
         this.logger = logger;

         this.configuration = configuration;
         this.persistence = persistence;
         this.state = new CategoryState(logger, this.configuration.Categories);
      }

      public async Task Run()
      {
         this.state.ClearDamageCategories();

         var indexes = await LoadTrackedByCategoryIndexes();
         await ContinueUnfinishedSagas(indexes);

         state.Update(indexes);

         RemovePendingEventsOutOfCategoryScope();
      }

      private async Task<List<Indexed>> LoadTrackedByCategoryIndexes() 
         => (await Task.WhenAll(this.state.Select(LoadCategoryIndex))).SelectMany(x => x).ToList();

      private async Task<List<Indexed>> LoadCategoryIndex((string Category, int Version) x)
         => (await LoadPersistedCategoryIndex(x)).Union(LoadPendingCategoryIndex(x.Category)).ToList();

      private IEnumerable<Indexed> LoadPendingCategoryIndex(string category)
         => this.pendingEvents.Where(evn => evn.Category == category);

      private Task<List<Indexed>> LoadPersistedCategoryIndex((string Category, int Version) x)
         => this.persistence.Load<Indexed>(this.persistence.CreateCategoryIndexStreamId(x.Category), x.Version + 1);

      private async Task ContinueUnfinishedSagas(IEnumerable<Indexed> events)
      {
         var sagasContinuation = CreateSagasContinuation(events);
         await ContinueSagas(sagasContinuation);
      }

      private IEnumerable<(Indexed Event, Func<Event, Task<ActionStatus>> Action)> CreateSagasContinuation(IEnumerable<Indexed> index)
      {
         var eventsToProcess = index
            .Where(KeepSagaEvents)
            .GroupBy(x => x.RefCorrelationId)
            .Select(g => BindActionToEvent(GetNewestEvent(g)));
         return eventsToProcess;
      }

      private bool KeepSagaEvents(Indexed evn)
         => this.configuration.IsKnownEventType(evn.RefType);

      private Indexed GetNewestEvent(IEnumerable<Indexed> group)
         => group.OrderByDescending(e => e.RefTimeStamp).First();

      private (Indexed Event, Func<Event, Task<ActionStatus>> Action) BindActionToEvent(Indexed evn)
         => (evn, this.configuration.GetAction(evn.RefType));

      private Task ContinueSagas(IEnumerable<(Indexed Event, Func<Event, Task<ActionStatus>> Action)> sagaTasks)
         => Task.WhenAll(sagaTasks.Select(x => ContinueSagaTask(x.Event, x.Action)));

      private async Task ContinueSagaTask(Indexed evn, Func<Event, Task<ActionStatus>> action)
      {
         var refEvent = await this.persistence.LoadEvent<Event>(evn.RefStreamId, evn.RefVersion);
         var result = await action(refEvent);
         if (result == ActionStatus.Error)
            ContinueOnError(evn);
         else if (result == ActionStatus.Pending)
            ContinueOnPending(evn);
         else
            ContinueOnOk(evn);
      }

      private void ContinueOnError(Indexed evn)
      {
         this.state.MarkCategoryAsDamage(evn.Category);
         this.logger.LogError(
            $"Saga {this.configuration.Name} failed to process event {evn}.");
      }

      private void ContinueOnPending(Indexed evn)
      {
         this.pendingEvents.Add(evn);
      }

      private void ContinueOnOk(Indexed evn)
      {
         this.pendingEvents.Remove(evn);
      }

      private void RemovePendingEventsOutOfCategoryScope()
         => this.pendingEvents.RemoveWhere(evn => !this.state.IsInScope(evn));
   }
}
