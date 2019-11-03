using System;
using System.Collections.Generic;
using Domain.Dispatch;
using Domain.Orders;
using Domain.Payment;

namespace Sagas.Common
{
   public class SagaConfiguration
   {
      private static readonly Dictionary<string, string> EventToCategory = new Dictionary<string, string>();
      private static void Register<TEvent, TState>() => EventToCategory.Add(typeof(TEvent).Name, typeof(TState).Name.ToLower());

      static SagaConfiguration()
      {
         Register<OrderItemAdded, Order>();
         Register<OrderCheckedOut, Order>();
         Register<OrderPaid, Payment>();
         Register<OrderDispatched, Dispatch>();
      }

      private readonly HashSet<string> trackedCategories = new HashSet<string>();
      private readonly Dictionary<string, ISagaAction> eventActions = new Dictionary<string, ISagaAction>();

      private void AddAnyAction<TEvent>(ISagaAction action)
      {
         var eventType = typeof(TEvent).Name;
         trackedCategories.Add(EventToCategory[eventType]);
         eventActions[eventType] = action;
      }

      public SagaConfiguration AddAction<TEvent>(ISagaAction action)
      {
         if (action == null) throw new ArgumentNullException(nameof(action));
         AddAnyAction<TEvent>(action);
         return this;
      }

      public SagaConfiguration AddEndAction<TEvent>()
      {
         AddAnyAction<TEvent>(null);
         return this;
      }

      public Optional<ISagaAction> GetSagaAction(string eventType)
      {
         if (this.eventActions.TryGetValue(eventType, out var action) && action != null)
            return new Some<ISagaAction>(action);
         return new None<ISagaAction>();
      }

      public bool IsKnownEventType(string eventType) => this.eventActions.ContainsKey(eventType);

      public IEnumerable<string> Categories => trackedCategories;
   }
}