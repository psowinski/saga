using System;
using System.Collections.Generic;
using System.Linq;

namespace Sagas.Common
{
   public class SagaConfiguration
   {
      private readonly Dictionary<string, Dictionary<string, ISagaAction>> register = new Dictionary<string, Dictionary<string, ISagaAction>>();

      private Dictionary<string, ISagaAction> UpsertCategoryActions(string category)
      {
         if (this.register.TryGetValue(category, out var actions)) return actions;
         actions = new Dictionary<string, ISagaAction>();
         this.register[category] = actions;
         return actions;
      }

      public SagaConfiguration AddAction<Category, Event>(ISagaAction action)
      {
         if (action == null) throw new ArgumentNullException(nameof(action));
         var categoryActions = UpsertCategoryActions(typeof(Category).Name.ToLower());
         categoryActions[typeof(Event).Name] = action;

         return this;
      }

      public SagaConfiguration AddEndAction<Category, Event>()
      {
         var categoryActions = UpsertCategoryActions(typeof(Category).Name.ToLower());
         categoryActions[typeof(Event).Name] = null;

         return this;
      }

      public Optional<ISagaAction> GetSagaAction(string category, string eventType)
      {
         if (this.register.TryGetValue(category, out var categoryActions)
            && categoryActions.TryGetValue(eventType, out var action) && action != null)
            return new Some<ISagaAction>(action);
         return new None<ISagaAction>();
      }

      public bool IsKnownEventType(string category, string eventType)
         => this.register.TryGetValue(category, out var categoryActions)
            && categoryActions != null && categoryActions.ContainsKey(eventType);

      public IEnumerable<string> Categories => register.Select(x => x.Key);
   }
}