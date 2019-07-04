using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;

namespace Runner
{
   public class CategoryProcess
   {
      public int LastProcessedVersion;
      public HashSet<ValueTuple<string, ISaga>> EventTypes;
   }

   public class SagaManager
   {
      private readonly Persistence persistence= new Persistence();
      private readonly Network network = new Network();

      private readonly Dictionary<string, CategoryProcess> register = new Dictionary<string, CategoryProcess>();
      public void RegisterSaga(string category, string eventType, ISaga saga)
      {
         if (this.register.TryGetValue(category, out var index))
            index.EventTypes.Add((eventType, saga));
         else
            this.register.Add(category, new CategoryProcess {EventTypes = new HashSet<ValueTuple<string, ISaga>> {(eventType, saga)}});
      }

      /*
       * Wake up on network / timer
       */
      private bool onNetwork;
      public async Task Run()
      {
         var idx = 0;
         while (true)
         {
            await Task.Delay(500);

            this.onNetwork = this.network.ReceiveLowValueMessage();
            var onTime = ++idx % 5 == 0;

            if (this.onNetwork || onTime)
               await Process();
         }
      }

      private async Task Process()
      {
         try
         {
            await Task.WhenAll(this.register.Select(ProcessCategory).ToList());
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
         }
      }

      private async Task ProcessCategory(KeyValuePair<string, CategoryProcess> reg)
      {
         var lastVersion = await this.persistence.GetLastStreamVersion(this.persistence.GetCategoryStreamId(reg.Key));
         if (reg.Value.LastProcessedVersion < lastVersion)
         {
            var why = onNetwork ? "network" : "time";
            Console.WriteLine($"Sagas: wake up by [{why}] event, category [{reg.Key}], processing {lastVersion - reg.Value.LastProcessedVersion} event(s) (ver. {reg.Value.LastProcessedVersion+1}-{lastVersion}).");
            await ProcessCategoryEvents(reg);
            reg.Value.LastProcessedVersion = lastVersion;
         }
      }

      private async Task ProcessCategoryEvents(KeyValuePair<string, CategoryProcess> reg)
      {
         var (category, process) = reg;
         var indexEvents = await this.persistence.Load(
            this.persistence.GetCategoryStreamId(category), process.LastProcessedVersion + 1);

         var eventsToProcess = indexEvents.Select(x =>
         {
            if (!(x is ByCategoryIndexEvent index)) return null;
            var saga = process.EventTypes.FirstOrDefault(e => e.Item1 == index?.RefType).Item2;
            return saga == null ? null : new {index.RefStreamId, index.RefVersion, saga};
         }).Where(x => x != null);

         foreach (var evn in eventsToProcess)
         {
            var refEvent = await this.persistence.LoadEvent(evn.RefStreamId, evn.RefVersion);
            await evn.saga.ProcessEvent(refEvent);
         }
      }
   }
}
