using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Aggregate;

namespace Saga
{
   public static class SagaUtil
   {
      public static Func<TEvent, Task<ActionStatus>> Protect<TEvent>(Func<TEvent, Task<ActionStatus>> action, List<string> errors) where TEvent : Event
      {
         async Task<ActionStatus> Safe(TEvent evn)
         {
            try
            {
               var result = await action(evn);
               if (result == ActionStatus.Error)
                  errors.Add($"Action fail for event {evn}");
               return result;
            }
            catch (Exception e)
            {
               errors.Add(e.ToString());
               return ActionStatus.Error;
            }
         }
         return Safe;
      }
   }
}