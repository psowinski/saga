using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Aggregate;

namespace Saga
{
   public enum ActionStatus
   {
      Ok,
      Pending,
      Error
   }

   public class SagaConfiguration
   {
      private readonly HashSet<string> trackedCategories = new HashSet<string>();
      private readonly Dictionary<string, Func<Event, Task<ActionStatus>>> eventActions = new Dictionary<string, Func<Event, Task<ActionStatus>>>();
      private readonly Func<Event, Task<ActionStatus>> DoNothing = _ => Task.FromResult(ActionStatus.Ok);

      public SagaConfiguration(string name)
      {
         Name = name;
      }

      public string Name { get; }

      private void AddAnyAction<TEvent>(Func<Event, Task<ActionStatus>> action) where TEvent : Event
      {
         var type = typeof(TEvent);
         var category = type.BaseType.GetProperty("Category").GetValue(null) as string;
         var eventType = type.Name;
         trackedCategories.Add(category);
         eventActions[eventType] = action;
      }

      public SagaConfiguration OnEvent<TEvent>(Func<TEvent, Task<ActionStatus>> action) where TEvent : Event
      {
         if (action == null) throw new ArgumentNullException(nameof(action));

         Task<ActionStatus> General(Event evn)
         {
            if (!(evn is TEvent specialized)) throw new ArgumentException(evn.ToString());
            return action(specialized);
         }

         AddAnyAction<TEvent>(General);
         return this;
      }

      public SagaConfiguration EndOnEvent<TEvent>() where TEvent : Event
      {
         AddAnyAction<TEvent>(DoNothing);
         return this;
      }

      public Func<Event, Task<ActionStatus>> GetAction(string eventType)
         => this.eventActions.TryGetValue(eventType, out var action) ? action : DoNothing;

      public bool IsKnownEventType(string eventType) => this.eventActions.ContainsKey(eventType);

      public IEnumerable<string> Categories => trackedCategories;
   }
}
