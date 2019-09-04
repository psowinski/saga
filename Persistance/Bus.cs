using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Common;

namespace Infrastructure
{
   public class Bus
   {
      private readonly Persistence persistence = new Persistence();
      private readonly Network network = new Network();

      public Task Pipe(Event evn) => Pipe(new[] {evn});

      public async Task Pipe(IEnumerable<Event> events)
      {
         await this.persistence.Save(events);
         this.network.SendSagaWakeup();
      }
   }
}
