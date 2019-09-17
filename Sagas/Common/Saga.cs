using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Domain.Common;
using Infrastructure;
using Infrastructure.Service;

namespace Sagas.Common
{
   public class Saga : ITask
   {
      private readonly Persistence persistence = new Persistence();
      private readonly SagaConfiguration configuration;
      private List<(string Category, int Version)> state;

      public Saga(SagaConfiguration configuration)
      {
         this.configuration = configuration;
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
         var (index, newState) = await LoadIndex();
         await ContinueUnfinishedSagas(index);
         UpdateState(newState);
      }

      private async Task<(IEnumerable<(string Category, Indexed Event)> Index,
         IEnumerable<(string Category, int Version)> State)> LoadIndex()
      {
         var categoryIndices = await Task.WhenAll(this.state.Select(LoadCategoryIndex).ToList());
         var index = categoryIndices.SelectMany(x => x.Index);
         var newState = categoryIndices.Select(x => (x.Category, Version: x.Index.Max(y => y.Event.Version)));
         return (index, newState);
      }

      private async Task<(IEnumerable<(string Category, Indexed Event)> Index, string Category)> LoadCategoryIndex(
         (string Category, int Version) x)
      {
         var categoryIndexEvents = await LoadCategoryIndexEvents(x.Category, x.Version + 1);
         return (categoryIndexEvents, x.Category);
      }

      public void UpdateState(IEnumerable<(string category, int version)> newCategoryState) =>
         this.state = newCategoryState.ToList();

      public void PrintCategoryStatus(IEnumerable<(string category, int version)> newCategoryState)
      {
         foreach (var item in newCategoryState)
            PrintCategoryStatus(item, this.state.Single(x => x.Category == item.category).Version);
      }
   
      private void PrintCategoryStatus((string Category, int Version) categoryWithVersion, int prevVersion)
      {
         Console.WriteLine(
            $"Processing {categoryWithVersion.Version - prevVersion} event(s) of category [{categoryWithVersion.Category}] (ver. {prevVersion + 1}-{categoryWithVersion.Version}).");
      }

      private async Task<List<(string Category, Indexed Event)>> LoadCategoryIndexEvents(string category, int fromVersion)
      {
         var index = await this.persistence.Load<Indexed>(this.persistence.GetCategoryIndexStreamId(category), fromVersion);
         return index.Select(x => (category, x)).ToList();
      }

      private async Task ContinueUnfinishedSagas(IEnumerable<(string Category, Indexed Event)> events)
      {
         var sagasContinuation = CreateSagasContinuation(events);
         await ContinueSagas(sagasContinuation);
      }

      private IEnumerable<(Indexed Event, ISagaAction Action)> CreateSagasContinuation(IEnumerable<(string Category, Indexed Event)> index)
      {
         var eventsToProcess = index
            .Where(KeepSagaEvents)
            .GroupBy(x => x.Event.RefCorrelationId)
            .Select(g => g.OrderByDescending(e => e.Event.RefTimeStamp).First())
            .Choose(BindActionToEvent);
         return eventsToProcess;
      }

      private bool KeepSagaEvents((string Category, Indexed Event) x)
         => this.configuration.IsKnownEventType(x.Category, x.Event.RefType);

      private Optional<(Indexed Event, ISagaAction Action)> BindActionToEvent((string Category, Indexed Event) arg)
         => this.configuration
            .GetSagaAction(arg.Category, arg.Event.RefType)
            .Map(x => (arg.Event, x));

      private Task ContinueSagas(IEnumerable<(Indexed Event, ISagaAction Action)> sagaTasks)
         => Task.WhenAll(sagaTasks.Select(x => ContinueSagaTask(x.Event, x.Action)).ToList());

      private async Task ContinueSagaTask(Indexed evn, ISagaAction action)
      {
         var refEvent = await this.persistence.LoadEvent<Event>(evn.RefStreamId, evn.RefVersion);
         await action.ProcessEvent(refEvent);
      }
   }
}
