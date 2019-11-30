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
      private readonly PendingEvents pendingEvents = new PendingEvents();
      private readonly IPersistenceClient persistence;
      private readonly SagaConfiguration configuration;
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

         var indexes = await LoadTrackedEvents();
         await ContinueUnfinishedSagas(indexes);

         state.Update(indexes);

         this.pendingEvents.ClampToState(this.state);
      }

      private async Task<List<Indexed>> LoadTrackedEvents() 
         => (await Task.WhenAll(this.state.Select(LoadCategoryIndex)))
            .SelectMany(x => x)
            .Union(this.pendingEvents.GetReadyEvents())
            .ToList();

      private Task<List<Indexed>> LoadCategoryIndex((string Category, int Version) x)
         => this.persistence.Load<Indexed>(this.persistence.CreateCategoryIndexStreamId(x.Category), x.Version + 1);

      private async Task ContinueUnfinishedSagas(IEnumerable<Indexed> events)
      {
         var sagasContinuation = CreateSagasContinuation(events);
         await ContinueSagas(sagasContinuation);
      }

      private IEnumerable<(Indexed Event, Func<Event, Task<ActionStatus>> Action)> CreateSagasContinuation(IEnumerable<Indexed> index)
      {
         bool KeepSagaEvents(Indexed evn) => this.configuration.IsKnownEventType(evn.RefType);
         Indexed GetNewestEvent(IEnumerable<Indexed> group) => group.OrderByDescending(e => e.RefTimeStamp).First();
         (Indexed Event, Func<Event, Task<ActionStatus>> Action) BindActionToEvent(Indexed evn)
            => (evn, this.configuration.GetAction(evn.RefType));

         var eventsToProcess = index
            .Where(KeepSagaEvents)
            .GroupBy(x => x.RefCorrelationId)
            .Select(g => BindActionToEvent(GetNewestEvent(g)));
         return eventsToProcess;
      }

      private Task ContinueSagas(IEnumerable<(Indexed Event, Func<Event, Task<ActionStatus>> Action)> sagaTasks)
         => Task.WhenAll(sagaTasks.Select(x => ContinueSagaTask(x.Event, x.Action)));

      private async Task ContinueSagaTask(Indexed evn, Func<Event, Task<ActionStatus>> action)
      {
         var refEvent = await this.persistence.LoadEvent<Event>(evn.RefStreamId, evn.RefVersion);
         var status = await action(refEvent);
         ProcessActionStatus(this.pendingEvents.ProcessActionStatus(status, evn), evn);
      }

      private void ProcessActionStatus(ActionStatus status, Indexed evn)
      {
         if (status != ActionStatus.Error) return;
         this.state.MarkCategoryAsDamage(evn.Category);
         this.logger.LogError(
            $"Saga {this.configuration.Name} failed to process event {evn}.");
      }
   }
}
