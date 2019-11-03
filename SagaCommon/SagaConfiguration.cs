using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Aggregate;

namespace Saga
{
   public class SagaConfiguration
   {
      private readonly HashSet<string> trackedCategories = new HashSet<string>();
      private readonly Dictionary<string, Func<Event, Task>> eventActions = new Dictionary<string, Func<Event, Task>>();
      private readonly Func<Event, Task> DoNothing = _ => Task.CompletedTask;
      private void AddAnyAction<TEvent>(Func<Event, Task> action) where TEvent : Event
      {
         var type = typeof(TEvent);
         var category = type.BaseType.GetProperty("Category").GetValue(null) as string;
         var eventType = type.Name;
         trackedCategories.Add(category);
         eventActions[eventType] = action;
      }

      public SagaConfiguration OnEvent<TEvent>(Func<TEvent, Task> action) where TEvent : Event
      {
         if (action == null) throw new ArgumentNullException(nameof(action));
         Func<Event, Task> general = evn =>
         {
            if (!(evn is TEvent specialized)) throw new ArgumentException(evn.ToString());
            return action(specialized);
         };
         AddAnyAction<TEvent>(general);
         return this;
      }

      public SagaConfiguration EndOnEvent<TEvent>() where TEvent : Event
      {
         AddAnyAction<TEvent>(DoNothing);
         return this;
      }

      public Func<Event, Task> GetAction(string eventType)
      {
         if (this.eventActions.TryGetValue(eventType, out var action))
            return action;
         return DoNothing;
      }

      public bool IsKnownEventType(string eventType) => this.eventActions.ContainsKey(eventType);

      public IEnumerable<string> Categories => trackedCategories;
   }
}
