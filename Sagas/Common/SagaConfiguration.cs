using System.Collections.Generic;
using System.Linq;

namespace Sagas.Common
{
   public class SagaConfiguration
   {
      private readonly Dictionary<string, Dictionary<string, ISagaAction>> register = new Dictionary<string, Dictionary<string, ISagaAction>>();

      private Dictionary<string, ISagaAction> GetCategoryActions(string category)
         => this.register.TryGetValue(category, out var actions) ? actions : null;

      private void AddCategoryActions(string category)
         => this.register[category] = new Dictionary<string, ISagaAction>();

      private Dictionary<string, ISagaAction> UpsertCategoryActions(string category)
      {
         if(GetCategoryActions(category) == null) AddCategoryActions(category);
         return GetCategoryActions(category);
      }

      public void AddAction(string category, string eventType, ISagaAction action)
      {
         var categoryActions = UpsertCategoryActions(category);
         categoryActions[eventType] = action;
      }

      public void AddEndAction(string category, string eventType)
      {
         var categoryActions = UpsertCategoryActions(category);
         categoryActions[eventType] = null;
      }

      public ISagaAction GetSagaAction(string category, string eventType)
      {
         var categoryActions = GetCategoryActions(category);
         if (categoryActions != null && categoryActions.TryGetValue(eventType, out var action))
            return action;
         return null;
      }

      public bool IsKnownEventType(string category, string eventType)
         => GetCategoryActions(category)?.ContainsKey(eventType) ?? false;

      public IEnumerable<string> Categories => register.Select(x => x.Key);
   }
}