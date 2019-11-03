using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Common;
using Infrastructure;

namespace Runner
{
   public class Bus
   {
      private static readonly Persistence persistence = new Persistence();
      private readonly Network network = new Network();

      public Task Pipe(Event evn) => Pipe(new[] {evn});

      public async Task Pipe(IEnumerable<Event> events)
      {
         await persistence.Save(events);
         this.network.SendSagaWakeup();
      }
   }
}
